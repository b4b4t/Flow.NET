using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flow.Store.Contracts;

/// <summary>
/// Store definition
/// </summary>
public interface IStoreDefinition
{
    /// <summary>
    /// Creat an instance to store the data.
    /// </summary>
    /// <returns>Data object</returns>
    object CreateDataInstance();

    /// <summary>
    /// Get value of the node in the data.
    /// </summary>
    /// <param name="data">Store data</param>
    /// <param name="node">Node name</param>
    /// <returns>Node data object</returns>
    object? GetValue(object data, string node);

    /// <summary>
    /// Set the node value in the store data.
    /// </summary>
    /// <param name="data">Store data</param>
    /// <param name="node">Node name</param>
    /// <param name="loader">Data loader</param>
    Task SetValueAsync(object data, string node, Func<object?, Task<object?>> loader);

    /// <summary>
    /// Get the node list of the store definition.
    /// </summary>
    /// <returns>Node list</returns>
    ICollection<string> GetNodes();
}

/// <summary>
/// Store definition
/// </summary>
public interface IStoreDefinition<TStore>
{
    /// <summary>
    /// Creat an instance to store the data.
    /// </summary>
    /// <returns>Data object</returns>
    TStore? CreateDataInstance();

    /// <summary>
    /// Get value of the node in the data.
    /// </summary>
    /// <param name="data">Store data</param>
    /// <param name="node">Node name</param>
    /// <returns>Node data object</returns>
    TNode? GetValue<TNode>(TStore data, string node);

    /// <summary>
    /// Set the node value in the store data.
    /// </summary>
    /// <param name="data">Store data</param>
    /// <param name="node">Node name</param>
    /// <param name="loader">Data loader</param>
    Task SetValueAsync<TNode>(TStore data, string node, Func<TNode?, Task<TNode?>> loader);

    /// <summary>
    /// Get the node list of the store definition.
    /// </summary>
    /// <returns>Node list</returns>
    ICollection<string> GetNodes();
}
