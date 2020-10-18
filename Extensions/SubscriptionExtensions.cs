using Flow.Subscription;
using System;
using System.Collections.Generic;

namespace Flow.Extensions
{
    public static class SubscriptionExtensions
    {
        public static void AddNodeSubscriptions(this StoreSubscription storeSubscription, ICollection<NodeSubscription> nodeSubscriptions)
        {
            if (storeSubscription is null)
            {
                throw new ArgumentNullException(nameof(storeSubscription));
            }

            if (nodeSubscriptions is null)
            {
                throw new ArgumentNullException(nameof(nodeSubscriptions));
            }

            foreach (var nodeSubscription in nodeSubscriptions)
            {
                storeSubscription.AddNodeSubription(nodeSubscription);
            }
        }
    }
}
