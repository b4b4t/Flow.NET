using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Flow.Subscription
{
    public class StoreSubscriber : IDisposable
    {
        /// <summary>
        /// Handle change node event.
        /// </summary>
        public event Action<string> HandleChangeNode;


        /// <summary>
        /// Handle change keys by node.
        /// </summary>
        private Dictionary<string, object> _eventKeys = new Dictionary<string, object>();

        /// <summary>
        /// Handle change actions by Even key.
        /// </summary>
        private EventHandlerList _handleChangeActions = new EventHandlerList();

        /// <summary>
        /// Subscriptions by node.
        /// </summary>        
        private Dictionary<string, ICollection<NodeSubscription>> _subscriptions = null;


        /// <summary>
        /// Default constructor
        /// </summary>
        public StoreSubscriber()
        {
            _subscriptions = new Dictionary<string, ICollection<NodeSubscription>>();
        }

        /// <summary>
        /// Constructor with nodes
        /// </summary>
        /// <param name="nodes">Store nodes</param>
        public StoreSubscriber(ICollection<string> nodes)
        {
            _subscriptions = new Dictionary<string, ICollection<NodeSubscription>>(nodes.Count);
        }

        /// <summary>
        /// Register a node
        /// </summary>
        /// <param name="node">Node name</param>
        public void RegisterNode(string node)
        {
            _subscriptions.Add(node, new List<NodeSubscription>());
            _eventKeys.Add(node, new object());
        }

        /// <summary>
        /// Add a subscription to a node.
        /// </summary>
        /// <param name="nodeSubscription">Node subscription</param>
        public void SubscribeToNode(NodeSubscription nodeSubscription)
        {
            if (nodeSubscription is null)
            {
                throw new ArgumentNullException(nameof(nodeSubscription));
            }

            string node = nodeSubscription.Node;

            CheckNodeSubscription(node);

            lock (_subscriptions)
            {
                _subscriptions[node].Add(nodeSubscription);
            }

            lock (_handleChangeActions)
            {
                _handleChangeActions.AddHandler(_eventKeys[node], nodeSubscription.HandleChange);
            }
        }

        /// <summary>
        /// Remove a subscription to a node.
        /// </summary>
        /// <param name="nodeSubscription">Node subscription</param>
        public void UnsubscribeToNode(NodeSubscription nodeSubscription)
        {
            if (nodeSubscription is null)
            {
                throw new ArgumentNullException(nameof(nodeSubscription));
            }

            string node = nodeSubscription.Node;

            CheckNodeSubscription(node);

            lock (_handleChangeActions)
            {
                _handleChangeActions.RemoveHandler(_eventKeys[node], nodeSubscription.HandleChange);
            }

            lock (_subscriptions)
            {
                _subscriptions[node].Remove(nodeSubscription);
            }
        }

        /// <summary>
        /// Emit a node change.
        /// </summary>
        /// <param name="node">Node name</param>
        public void EmitNodeChange(string node)
        {
            CheckNodeSubscription(node);

            HandleChangeNode(node);
        }

        /// <summary>
        /// Get the event handler for a node.
        /// </summary>
        /// <param name="node">Node name</param>
        /// <returns></returns>
        public Action GetNodeHandleChange(string node)
        {
            CheckNodeSubscription(node);

            return (Action)_handleChangeActions[_eventKeys[node]];
        }

        /// <summary>
        /// Dispose the store subscriber.
        /// </summary>
        public void Dispose()
        {
            _handleChangeActions.Dispose();
        }

        /// <summary>
        /// Check if the node exists in the subscriptions.
        /// </summary>
        /// <param name="node">Node name</param>
        private void CheckNodeSubscription(string node)
        {
            if (!_subscriptions.ContainsKey(node))
            {
                throw new ArgumentException($"Node does not exist : {node}");
            }
        }
    }
}
