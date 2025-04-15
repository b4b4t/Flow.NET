using System;
using System.Threading.Tasks;

namespace Flow.Store.Contracts;

/// <summary>
/// Store node
/// </summary>
public interface IStoreNode<TNode>
{
    /// <summary>
    /// If the node value is loading
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Last update date.
    /// </summary>
    DateTime? LastUpdateDate { get; }

    /// <summary>
    /// Node value.
    /// </summary>
    TNode? Value { get; }

    /// <summary>
    /// Load the node data and store it in the Value property
    /// </summary>
    /// <param name="loader">Loader</param>
    /// <returns></returns>
    Task LoadAsync(Func<TNode?, Task<TNode?>> loader);
}
