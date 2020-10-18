using System;

namespace Flow.Subscription
{
    public class NodeSubscription
    {
        public NodeSubscription(string node, Action handleChange)
        {
            Node = node;
            HandleChange = handleChange;
        }

        public string Node { get; private set; }
        public Action HandleChange { get; private set; }
    }
}
