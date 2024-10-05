using System.Collections.Generic;

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
    object GetValue(object data, string node);

    /// <summary>
    /// Set the node value in the store data.
    /// </summary>
    /// <param name="data">Store data</param>
    /// <param name="node">Node name</param>
    /// <param name="value">Data object</param>
    void SetValue(object data, string node, object value);

    /// <summary>
    /// Get the node list of the store definition.
    /// </summary>
    /// <returns>Node list</returns>
    ICollection<string> GetNodes();
}
