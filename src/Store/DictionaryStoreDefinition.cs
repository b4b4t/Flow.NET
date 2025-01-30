using Flow.Store.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Store;

/// <summary>
/// Default constructor.
/// </summary>
/// <param name="nodes">Node list</param>
public class DictionaryStoreDefinition(string[] nodes) : IStoreDefinition
{
    /// <summary>
    /// Node list.
    /// </summary>
    private string[] _nodes = nodes;

    /// <inheritdoc cref="IStoreDefinition.CreateDataInstance"/>
    public object CreateDataInstance()
    {
        Dictionary<string, object?> data = new (_nodes.Length);

        foreach (string node in _nodes)
        {
            data.Add(node, null);
        }

        return data;
    }

    /// <inheritdoc cref="IStoreDefinition.GetNodes"/>
    public ICollection<string> GetNodes() => _nodes;

    /// <inheritdoc cref="IStoreDefinition.GetValue(object, string)"/>
    public object? GetValue(object data, string node)
    {
        CheckNode(node);

        IDictionary<string, object?>? storeData = data as IDictionary<string, object?> 
            ?? throw new InvalidOperationException("No store data found");
        
        return storeData[node];
    }

    /// <inheritdoc cref="IStoreDefinition.SetValue(object, string, object?)"/>
    public void SetValue(object data, string node, object? value)
    {
        CheckNode(node);

        IDictionary<string, object?>? storeData = data as IDictionary<string, object?>
            ?? throw new InvalidOperationException("No store data found");

        storeData[node] = value;
    }

    /// <summary>
    /// Check if the node exists in the store definition.
    /// </summary>
    /// <param name="node">Node name</param>
    private void CheckNode(string node)
    {
        if (!_nodes.Contains(node))
        {
            throw new ArgumentException($"Node {node} does not exist");
        }
    }
}
