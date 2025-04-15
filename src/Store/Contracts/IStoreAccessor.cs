using System.Threading.Tasks;

namespace Flow.Store.Contracts;

/// <summary>
/// Store
/// </summary>
public interface IStoreManager
{
    /// <summary>
    /// Get a node value
    /// </summary>
    /// <param name="node">Node name</param>
    /// <returns>Node value</returns>
    object? GetNodeValue(string node);

    /// <summary>
    /// Dispatch an action
    /// </summary>
    /// <param name="action">Action</param>
    Task DispatchAsync(IAction action);
}

/// <summary>
/// Store
/// </summary>
public interface IStoreManager<TStore>
{
    /// <summary>
    /// Get a node value
    /// </summary>
    /// <param name="node">Node name</param>
    /// <returns>Node value</returns>
    TNode? GetNodeValue<TNode>(string node);

    /// <summary>
    /// Dispatch an action
    /// </summary>
    /// <param name="action">Action</param>
    Task DispatchAsync<TNode>(IAction<TNode> action);
}
