using Flow.Subscription;
using System;
using System.Collections.Generic;

namespace Flow.Extensions;

/// <summary>
/// Subscription extensions
/// </summary>
public static class SubscriptionExtensions
{
    /// <summary>
    /// Add node subscriptions into a store subscription.
    /// </summary>
    /// <param name="storeSubscription">Store subscription</param>
    /// <param name="nodeSubscriptions">Node subscriptions</param>
    /// <exception cref="ArgumentNullException">Error when one of the argument is null</exception>
    public static void AddNodeSubscriptions(this StoreSubscription storeSubscription, ICollection<NodeSubscription> nodeSubscriptions)
    {
        ArgumentNullException.ThrowIfNull(storeSubscription);
        ArgumentNullException.ThrowIfNull(nodeSubscriptions);

        foreach (var nodeSubscription in nodeSubscriptions)
        {
            storeSubscription.AddNodeSubription(nodeSubscription);
        }
    }
}
