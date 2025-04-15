using Flow.Store.Contracts;
using System;
using System.Threading.Tasks;

namespace Flow.Store;

/// <summary>
/// Default store node.
/// </summary>
/// <typeparam name="TNode">Node value type</typeparam>
public class StoreNode<TNode> : IStoreNode<TNode>
{
    /// <summary>
    /// If the node value is loading.
    /// </summary>
    private bool _isLoading = false;

    /// <inheritdoc />
    public bool IsLoading => _isLoading;

    /// <summary>
    /// Last update date.
    /// </summary>
    private DateTime? _lastUpdateDate;

    /// <summary>
    /// Last update date.
    /// </summary>
    public DateTime? LastUpdateDate => _lastUpdateDate;

    /// <summary>
    /// Node value.
    /// </summary>
    public TNode? Value => _value;

    /// <summary>
    /// Node value.
    /// </summary>
    private TNode? _value;

    /// <summary>
    /// Load the state.
    /// </summary>
    /// <returns>Task.</returns>
    public async Task LoadAsync(Func<TNode?, Task<TNode?>> loader)
    {
        _isLoading = true;

        try
        {
            _value = await loader(Value);
            _lastUpdateDate = DateTime.Now;
        }
        finally
        {
            _isLoading = false;
        }
    }
}
