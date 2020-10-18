using System;
using System.Collections.Generic;

namespace Flow.Subscription
{
    public class StoreSubscription
    {
        public StoreSubscription(string identifier)
        {
            Identifier = identifier;
            NodeSubscriptions = new List<NodeSubscription>();
        }

        public string Identifier { get; private set; }
        public ICollection<NodeSubscription> NodeSubscriptions { get; private set; }

        public void AddNodeSubription(NodeSubscription nodeSubscription)
        {
            if (nodeSubscription is null)
            {
                throw new ArgumentNullException(nameof(nodeSubscription));
            }

            NodeSubscriptions.Add(nodeSubscription);
        }
    }
}
