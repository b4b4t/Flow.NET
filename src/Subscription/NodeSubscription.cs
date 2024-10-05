using System;

namespace Flow.Subscription;

/// <summary>
/// Node subscription
/// </summary>
public class NodeSubscription(string node, Action handleChange)
{
    /// <summary>
    /// Node
    /// </summary>
    public string Node { get; private set; } = node;
    
    /// <summary>
    /// Handle change action
    /// </summary>
    public Action HandleChange { get; private set; } = handleChange;
}
