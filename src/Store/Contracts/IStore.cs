using Flow.Subscription;
using System.Collections.Generic;

namespace Flow.Store.Contracts;

/// <summary>
/// Store base
/// </summary>
public interface IStore
{
    /// <summary>
    /// Store identifier
    /// </summary>
    string Identifier { get; }

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
}
