using Flow.Store;
using Flow.Store.Contracts;

namespace BlazorWebAssemplyApp.Stores;

/// <summary>
/// Typed store
/// </summary>
public class TypedStore
{
    /// <summary>
    /// Typed store counter
    /// </summary>
    public int TypedStoreCounterNode { get; private set; }

    /// <summary>
    /// Store node counter
    /// </summary>
    public IStoreNode<int> StoreNodeCounterNode { get; private set; } = new StoreNode<int>();
}
