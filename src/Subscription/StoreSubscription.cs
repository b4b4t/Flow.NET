using System;
using System.Collections.Generic;

namespace Flow.Subscription;

/// <summary>
/// Store subscription
/// </summary>
/// <param name="identifier">Store identifier</param>
public class StoreSubscription(string identifier)
{
    /// <summary>
    /// Store identifier
    /// </summary>
    public string Identifier { get; private set; } = identifier;

    /// <summary>
    /// Node subscriptions of the store
    /// </summary>
    public ICollection<NodeSubscription> NodeSubscriptions { get; private set; } = [];

    /// <summary>
    /// Add a subscription to a node
    /// </summary>
    /// <param name="nodeSubscription">Node subscription</param>
    /// <exception cref="ArgumentNullException">Node subscription is null</exception>
    public void AddNodeSubription(NodeSubscription nodeSubscription)
    {
        ArgumentNullException.ThrowIfNull(nodeSubscription);

        NodeSubscriptions.Add(nodeSubscription);
    }
}
