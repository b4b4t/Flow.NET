using Flow.Store.Contracts;

namespace Flow.Store;

/// <summary>
/// Store action
/// </summary>
public class StoreAction(string identifier, string node, object data) : IAction
{
    /// <summary>
    /// Store identifier
    /// </summary>
    public string Identifier { get; set; } = identifier;

    /// <summary>
    /// Node name
    /// </summary>
    public string Node { get; set; } = node;

    /// <summary>
    /// Data
    /// </summary>
    public object Data { get; set; } = data;
}

/// <summary>
/// Store action
/// </summary>
public class StoreAction<TNode>(string identifier, string node, TNode data) : IAction<TNode>
{
    /// <summary>
    /// Store identifier
    /// </summary>
    public string Identifier { get; set; } = identifier;

    /// <summary>
    /// Node name
    /// </summary>
    public string Node { get; set; } = node;

    /// <summary>
    /// Data
    /// </summary>
    public TNode Data { get; set; } = data;
}
