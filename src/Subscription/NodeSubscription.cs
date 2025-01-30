using System;
using System.Threading.Tasks;

namespace Flow.Subscription;

/// <summary>
/// Node subscription
/// </summary>
public class NodeSubscription(string node, Func<Task> handleChange)
{
    /// <summary>
    /// Node
    /// </summary>
    public string Node { get; private set; } = node;
    
    /// <summary>
    /// Handle change action
    /// </summary>
    public Func<Task> HandleChange { get; private set; } = handleChange;
}
