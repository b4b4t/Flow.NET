using Flow.Exceptions;
using Flow.Store.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Flow.Store;

/// <summary>
/// Typed store definition.
/// </summary>
/// <typeparam name="TStore">Store type</typeparam>
/// <param name="store">Store value</param>
public class TypedStoreDefinition<TStore> : IStoreDefinition<TStore> where TStore : new()
{
    /// <summary>
    /// Node list.
    /// </summary>
    private readonly string[] _nodes;

    /// <summary>
    /// Property getters
    /// </summary>
    private readonly Dictionary<string, Delegate> _propertyGetters = [];

    /// <summary>
    /// Property setters
    /// </summary>
    private readonly Dictionary<string, Delegate> _propertySetters = [];

    /// <summary>
    /// Default constructor
    /// </summary>
    public TypedStoreDefinition()
    {
        Type storeNodeType = typeof(IStoreNode<>);
        PropertyInfo[]? properties = typeof(TStore).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        _nodes = properties.Select(p => p.Name).ToArray();

        foreach (PropertyInfo property in properties)
        {
            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == storeNodeType)
            {
                RegisterStoreNodeProperty(property);
            }
            else
            {
                RegisterProperty(property);
            }
        }
    }

    /// <inheritdoc />
    public TStore? CreateDataInstance() => new();

    /// <inheritdoc />
    public ICollection<string> GetNodes() => _nodes;

    /// <inheritdoc />
    public TNode GetValue<TNode>(TStore store, string node)
    {
        if (_propertyGetters.TryGetValue(node, out Delegate? getter))
        {
            if (getter is Func<TStore, TNode> typedGetter)
            {
                return typedGetter(store);
            }
            else
            {
                throw new InvalidOperationException($"Getter for '{node}' is not of type Func<{typeof(TStore).Name}, {typeof(TNode).Name}>.");
            }
        }

        throw new InvalidOperationException($"{node} getter not found for {typeof(TStore).Name}");
    }

    /// <inheritdoc />
    public async Task SetValueAsync<TNode>(TStore store, string node, Func<TNode?, Task<TNode?>> loader)
    {
        if (_propertySetters.TryGetValue(node, out Delegate? setter))
        {
            if (setter is Func<TStore, Func<TNode?, Task<TNode?>>, Task> typedSetter && typedSetter is not null)
            {
                await typedSetter(store, loader);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Setter for '{node}' is not of type Func<{typeof(TStore).Name}, Func<{typeof(TNode).Name}?, Task<{typeof(TNode).Name}>>, Task> (Found type : {setter.GetType().Name}).");
            }
        }
        else
        {
            throw new InvalidOperationException($"{node} setter not found for {typeof(TStore).Name}");
        }
    }

    /// <summary>
    /// Register the setter and getter for the property.
    /// </summary>
    /// <param name="property">Node property</param>
    private void RegisterProperty(PropertyInfo property)
    {
        // Getter (data) => data.NodeValue
        var parameter = Expression.Parameter(typeof(TStore), "data");
        var propertyAccess = Expression.Property(parameter, property); // data.NodeValue
        var getterType = typeof(Func<,>).MakeGenericType(typeof(TStore), property.PropertyType);
        var getterLambda = Expression.Lambda(getterType, propertyAccess, parameter);
        var getter = getterLambda.Compile();
        _propertyGetters.Add(property.Name, getter);

        // Setter (data, loader) => data.NodeValue = await loader(data.NodeValue)
        // Check if the setter is non public
        MethodInfo? setMethod = property.GetSetMethod(true) ?? throw new InvalidPropertyException(typeof(TStore).Name, property.Name, false);
        bool isInitOnly = setMethod.ReturnParameter?.GetRequiredCustomModifiers()
                          .Contains(typeof(IsExternalInit)) ?? false;

        if (!setMethod.IsPrivate && !isInitOnly)
        {
            throw new InvalidPropertyException(typeof(TStore).Name, property.Name, setMethod.IsPublic);
        }

        // loader parameter
        var loaderParam = Expression.Parameter(typeof(Func<,>).MakeGenericType(property.PropertyType, typeof(Task<>).MakeGenericType(property.PropertyType)), "loader");

        // Create the expression to call the loader with the current value
        var callLoader = Expression.Invoke(loaderParam, propertyAccess);

        // TaskAwaiter<T>
        var awaiterMethod = typeof(Task<>)
            .MakeGenericType(property.PropertyType)
            .GetMethod(nameof(Task<object>.GetAwaiter))!;
        var awaiterType = awaiterMethod.ReturnType;

        // .GetAwaiter() call
        var getAwaiter = Expression.Call(callLoader, awaiterMethod);

        // .GetResult() call
        var getResult = Expression.Call(getAwaiter, awaiterType.GetMethod(nameof(TaskAwaiter<object>.GetResult))!);

        // Create the expression to set the property
        var assignProperty = Expression.Assign(propertyAccess, getResult);

        // Create the lambda expression Func<TStore, Func<TNode?, Task<TNode?>>, Task>
        var setter = Expression.Lambda(
            typeof(Func<,,>).MakeGenericType(
                // Store parameter type
                typeof(TStore), 
                // Loader parameter type (Func<TNode, Task<TNode>)
                typeof(Func<,>).MakeGenericType(
                    property.PropertyType, 
                    typeof(Task<>).MakeGenericType(property.PropertyType)
                ),
                // Value return type (Task<TNode>)
                typeof(Task)),
            Expression.Block(typeof(Task), assignProperty, Expression.Constant(Task.CompletedTask)), // mimic async by returning completed Task
            parameter,
            loaderParam
        ).Compile();

        _propertySetters.Add(property.Name, setter);
    }

    private void RegisterStoreNodeProperty(PropertyInfo property)
    {
        // Get the node object
        PropertyInfo valueProperty = property.PropertyType.GetProperty(nameof(IStoreNode<object>.Value))!;
        var parameter = Expression.Parameter(typeof(TStore), "data");
        var nodePropertyAccess = Expression.Property(parameter, property); // data.NodeValue
        var propertyAccess = Expression.Property(nodePropertyAccess, valueProperty); // data.NodeValue.Value
        // Create the exception to throw
        var exception = Expression.New(typeof(NodePropertyException).GetConstructor([typeof(string)])!, 
            Expression.Constant($"{property.Name} is null. The property {property.Name} must be initialized in the {typeof(TStore).Name} store class."));
        // Create the conditional expression
        var condition = Expression.Condition(
            Expression.NotEqual(nodePropertyAccess, Expression.Constant(null, property.PropertyType)),
            propertyAccess,
            Expression.Throw(exception, valueProperty.PropertyType) // Ensure the type matches the return type
        );

        // Getter (data) => data.NodeValue?.Value ?? throw new Exception("")
        var getterType = typeof(Func<,>).MakeGenericType(typeof(TStore), valueProperty.PropertyType);
        var getterLambda = Expression.Lambda(getterType, condition, parameter);
        var getter = getterLambda.Compile();
        _propertyGetters.Add(property.Name, getter);

        // Get the setter by calling the LoadAsync method of IStoreNode
        // Setter (data, loader) => await LoadAsync(loader);
        // Check if the setter is non public
        MethodInfo? setMethod = property.GetSetMethod(true) ?? throw new InvalidPropertyException(typeof(TStore).Name, property.Name, false);
        bool isInitOnly = setMethod.ReturnParameter?.GetRequiredCustomModifiers()
                          .Contains(typeof(IsExternalInit)) ?? false;

        if (!setMethod.IsPrivate && !isInitOnly)
        {
            throw new InvalidPropertyException(typeof(TStore).Name, property.Name, setMethod.IsPublic);
        }

        // loader parameter
        var loaderParam = Expression.Parameter(typeof(Func<,>).MakeGenericType(valueProperty.PropertyType, typeof(Task<>).MakeGenericType(valueProperty.PropertyType)), "loader");

        // Get the LoadAsync method of the node
        var loadAsyncMethod = property.PropertyType.GetMethod(nameof(IStoreNode<object>.LoadAsync))!;
        var callLoad = Expression.Call(nodePropertyAccess, loadAsyncMethod, loaderParam);

        // TaskAwaiter<T>
        var awaiterMethod = typeof(Task)
            .GetMethod(nameof(Task.GetAwaiter))!;
        var awaiterType = awaiterMethod.ReturnType;

        // .GetAwaiter() call
        var getAwaiter = Expression.Call(callLoad, awaiterMethod);

        // .GetResult() call
        var getResult = Expression.Call(getAwaiter, awaiterType.GetMethod(nameof(TaskAwaiter.GetResult))!);

        // Create the lambda expression Func<TStore, Func<TNode?, Task<TNode?>>, Task>
        var setter = Expression.Lambda(
            typeof(Func<,,>).MakeGenericType(
                // Store parameter type
                typeof(TStore),
                // Loader parameter type (Func<TNode, Task<TNode>)
                typeof(Func<,>).MakeGenericType(
                    valueProperty.PropertyType,
                    typeof(Task<>).MakeGenericType(valueProperty.PropertyType)
                ),
                // Value return type (Task<TNode>)
                typeof(Task)),
            Expression.Block(typeof(Task), getResult, Expression.Constant(Task.CompletedTask)), // mimic async by returning completed Task
            parameter,
            loaderParam
        ).Compile();

        _propertySetters.Add(property.Name, setter);
    }
}
