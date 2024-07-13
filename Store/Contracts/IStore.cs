using Flow.Subscription;
using System.Collections.Generic;

namespace Flow.Store.Contracts;

/// <summary>
/// Store
/// </summary>
public interface IStore
{
    /// <summary>
    /// Store identifier
    /// </summary>
    string Identifier { get; }

    /// <summary>
    /// Get a node value
    /// </summary>
    /// <param name="node">Node name</param>
    /// <returns>Node value</returns>
    object GetNodeValue(string node);

    /// <summary>
    /// Connect the subscriptions to a store
    /// </summary>
    /// <param name="nodeSubscriptions">Node subscriptions</param>
    void ConnectToStore(ICollection<NodeSubscription> nodeSubscriptions);
    
    /// <summary>
    /// Disconnect the subscrptions to a store
    /// </summary>
    /// <param name="nodeSubscriptions">Node subscriptions</param>
    void DisconnectToStore(ICollection<NodeSubscription> nodeSubscriptions);

    /// <summary>
    /// Dispatch an action
    /// </summary>
    /// <param name="action">Action</param>
    void Dispatch(IAction action);
}
