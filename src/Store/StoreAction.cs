using Flow.Store.Contracts;
using System;
using System.Threading.Tasks;

namespace Flow.Store;

/// <summary>
/// Store action
/// </summary>
public class StoreAction: IAction
{
    /// <summary>
    /// Store identifier
    /// </summary>
    public string Identifier { get; init; }

    /// <summary>
    /// Node name
    /// </summary>
    public string Node { get; init; }

    /// <summary>
    /// Node loader
    /// </summary>
    public Func<object?, Task<object?>> Loader { get; init; }

    /// <summary>
    /// Create a store action by passing the node data.
    /// </summary>
    /// <param name="identifier">Store identifier</param>
    /// <param name="node">Node identifier</param>
    /// <param name="data">Node data</param>
    public StoreAction(string identifier, string node, object? data)
    {
        Identifier = identifier;
        Node = node;
        Loader = (_) => Task.FromResult(data);
    }

    /// <summary>
    /// Create a store action by passing the node data loader.
    /// </summary>
    /// <param name="identifier">Store identifier</param>
    /// <param name="node">Node identifier</param>
    /// <param name="loader">Node data</param>
    public StoreAction(string identifier, string node, Func<object?, Task<object?>> loader)
    {
        Identifier = identifier;
        Node = node;
        Loader = loader;
    }
}

/// <summary>
/// Store action
/// </summary>
public class StoreAction<TNode> : IAction<TNode>
{
    /// <summary>
    /// Store identifier
    /// </summary>
    public string Identifier { get; init; }

    /// <summary>
    /// Node name
    /// </summary>
    public string Node { get; init; }

    /// <summary>
    /// Node loader
    /// </summary>
    public Func<TNode?, Task<TNode?>> Loader { get; init; }

    /// <summary>
    /// Create a store action by passing the node data.
    /// </summary>
    /// <param name="identifier">Store identifier</param>
    /// <param name="node">Node identifier</param>
    /// <param name="data">Node data</param>
    public StoreAction(string identifier, string node, TNode? data)
    {
        Identifier = identifier;
        Node = node;
        Loader = (_) =>  Task.FromResult(data);
    }

    /// <summary>
    /// Create a store action by passing the node data loader.
    /// </summary>
    /// <param name="identifier">Store identifier</param>
    /// <param name="node">Node identifier</param>
    /// <param name="loader">Node data</param>
    public StoreAction(string identifier, string node, Func<TNode?, Task<TNode?>> loader)
    {
        Identifier = identifier;
        Node = node;
        Loader = loader;
    }
}