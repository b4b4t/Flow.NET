using Flow.Store.Contracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Flow.Store;

/// <summary>
/// Store definition
/// </summary>
/// <typeparam name="T"></typeparam>
public class StoreDefinition<T> : IStoreDefinition where T: class, new()
{
    /// <summary>
    /// Node properties
    /// </summary>
    private readonly Dictionary<string, PropertyInfo> _nodeProperties;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public StoreDefinition()
    {
        PropertyInfo[] properties = typeof(T).GetProperties();

        _nodeProperties = new Dictionary<string, PropertyInfo>(properties.Length);

        foreach (var property in properties)
        {
            _nodeProperties.Add(property.Name, property);
        }
    }

    /// <inheritdoc cref="IStoreDefinition.CreateDataInstance"/>
    public object CreateDataInstance() => new T();

    /// <inheritdoc cref="IStoreDefinition.GetNodes"/>
    public ICollection<string> GetNodes() => _nodeProperties.Keys;

    /// <inheritdoc cref="IStoreDefinition.GetValue(object, string)"/>
    public object? GetValue(object data, string node)
    {
        CheckNode(node);

        return _nodeProperties[node].GetValue(data);
    }

    /// <inheritdoc cref="IStoreDefinition.SetValueAsync(object, string, Func{object?, Task{object?}})"/>
    public async Task SetValueAsync(object data, string node, Func<object?, Task<object?>> loader)
    {
        CheckNode(node);

        PropertyInfo property = _nodeProperties[node];
        object? value = await loader(property.GetValue(data));

        property.SetValue(data, value);
    }

    /// <summary>
    /// Check if the node exists in the store definition.
    /// </summary>
    /// <param name="node">Node name</param>
    private void CheckNode(string node)
    {
        if (!_nodeProperties.ContainsKey(node))
        {
            throw new ArgumentException($"Node {node} does not exist");
        }
    }
}
