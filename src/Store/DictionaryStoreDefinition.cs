using Flow.Store.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        Dictionary<string, IStoreNode<object?>> data = new (_nodes.Length);

        foreach (string node in _nodes)
        {
            data.Add(node, new StoreNode<object?>());
        }

        return data;
    }

    /// <inheritdoc cref="IStoreDefinition.GetNodes"/>
    public ICollection<string> GetNodes() => _nodes;

    /// <inheritdoc cref="IStoreDefinition.GetValue(object, string)"/>
    public object? GetValue(object data, string node)
    {
        CheckNode(node);

        IDictionary<string, IStoreNode<object?>>? storeData = data as IDictionary<string, IStoreNode<object?>> 
            ?? throw new InvalidOperationException("No store data found");
        
        return storeData[node].Value;
    }

    /// <inheritdoc cref="IStoreDefinition.SetValueAsync(object, string, Func{object?, Task{object?}})"/>
    public async Task SetValueAsync(object data, string node, Func<object?, Task<object?>> loader)
    {
        CheckNode(node);

        IDictionary<string, IStoreNode<object?>>? storeData = data as IDictionary<string, IStoreNode<object?>>
            ?? throw new InvalidOperationException("No store data found");

        await storeData[node].LoadAsync(loader);
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
