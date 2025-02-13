using Flow.Store.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        var properties = typeof(TStore).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        _nodes = properties.Select(p => p.Name).ToArray();

        foreach (PropertyInfo property in properties)
        {
            // Getter
            var parameter = Expression.Parameter(typeof(TStore), "data");
            var propertyAccess = Expression.Property(parameter, property);
            var getterType = typeof(Func<,>).MakeGenericType(typeof(TStore), property.PropertyType);
            var getter = Expression.Lambda(getterType, propertyAccess, parameter).Compile();

            _propertyGetters.Add(property.Name, getter);

            // Setter
            var valueParameter = Expression.Parameter(property.PropertyType, "value");
            var setter = Expression.Lambda(
                typeof(Action<,>).MakeGenericType(typeof(TStore), property.PropertyType),
                Expression.Assign(propertyAccess, valueParameter),
                parameter,
                valueParameter
            ).Compile();

            _propertySetters.Add(property.Name, setter);
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
    public void SetValue<TNode>(TStore store, string node, TNode value)
    {
        if (_propertySetters.TryGetValue(node, out Delegate? setter))
        {
            if (setter is Action<TStore, TNode> typedSetter && typedSetter is not null)
            {
                typedSetter(store, value);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Setter for '{node}' is not of type Func<{typeof(TStore).Name}, {typeof(TNode).Name}> (Found type : {setter.GetType().Name}).");
            }
        }
        else
        {
            throw new InvalidOperationException($"{node} setter not found for {typeof(TStore).Name}");
        }
    }
}
