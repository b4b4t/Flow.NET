using System.Threading.Tasks;
using System;

namespace Flow.Store.Contracts;

/// <summary>
/// Action interface
/// </summary>
public interface IAction : IAction<object>;

/// <summary>
/// Action base interface
/// </summary>
public interface IAction<TNode>
{
    /// <summary>
    /// Store identifier
    /// </summary>
    string Identifier { get; init; }

    /// <summary>
    /// Node name
    /// </summary>
    string Node { get; init; }

    /// <summary>
    /// Loader to 
    /// </summary>
    Func<TNode?, Task<TNode?>> Loader { get; init; }
}