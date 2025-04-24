using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Flow.Subscription;

/// <summary>
/// Store subscriber
/// </summary>
public class StoreSubscriber : IDisposable
{
    /// <summary>
    /// Handle change node event.
    /// </summary>
    public event Func<string, Task> HandleChangeNode = null!;

    /// <summary>
    /// Handle change keys by node.
    /// </summary>
    private Dictionary<string, object> _eventKeys = [];

    /// <summary>
    /// Handle change actions by Even key.
    /// </summary>
    private EventHandlerList _handleChangeActions = new ();

    /// <summary>
    /// Subscriptions by node.
    /// </summary>        
    private Dictionary<string, ICollection<NodeSubscription>> _subscriptions;

    /// <summary>
    /// Default constructor
    /// </summary>
    public StoreSubscriber() => _subscriptions = [];

    /// <summary>
    /// Constructor with nodes
    /// </summary>
    /// <param name="nodes">Store nodes</param>
    public StoreSubscriber(ICollection<string> nodes) 
        => _subscriptions = new Dictionary<string, ICollection<NodeSubscription>>(nodes.Count);

    /// <summary>
    /// Register a node
    /// </summary>
    /// <param name="node">Node name</param>
    public void RegisterNode(string node)
    {
        _subscriptions.Add(node, []);
        _eventKeys.Add(node, new object());
    }

    /// <summary>
    /// Add a subscription to a node.
    /// </summary>
    /// <param name="nodeSubscription">Node subscription</param>
    public void SubscribeToNode(NodeSubscription nodeSubscription)
    {
        ArgumentNullException.ThrowIfNull(nodeSubscription);

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
        ArgumentNullException.ThrowIfNull(nodeSubscription);

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
    public async Task EmitNodeChangeAsync(string node)
    {
        CheckNodeSubscription(node);

        await HandleChangeNode(node);
    }

    /// <summary>
    /// Get the event handler for a node.
    /// </summary>
    /// <param name="node">Node name</param>
    /// <returns></returns>
    public Func<Task>? GetNodeHandleChange(string node)
    {
        CheckNodeSubscription(node);

        return _handleChangeActions[_eventKeys[node]] as Func<Task>;
    }

    /// <summary>
    /// Dispose the store subscriber.
    /// </summary>
    public void Dispose() => _handleChangeActions.Dispose();

    /// <summary>
    /// Check if the node exists in the subscriptions.
    /// </summary>
    /// <param name="node">Node name</param>
    private void CheckNodeSubscription(string node)
    {
        if (!_subscriptions.ContainsKey(node))
        {
            throw new ArgumentException($"Node does not exist", node);
        }
    }
}
