using Flow.Store.Contracts;
using Flow.Subscription;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flow.Store;

/// <summary>
/// Store
/// </summary>
public class Store : IStore, IStoreManager, IDisposable
{
    /// <summary>
    /// Data of the store.
    /// </summary>
    public object Data { get; private set; }

    /// <summary>
    /// Identifier
    /// </summary>
    public string Identifier { get; private set; }

    /// <summary>
    /// Store definition
    /// </summary>
    private readonly IStoreDefinition _storeDefinition;

    /// <summary>
    /// Store subscriber.
    /// </summary>
    private readonly StoreSubscriber _storeSubscriber;

    /// <summary>
    /// Lock for the data
    /// </summary>
    private readonly object _dataLock = new();

    /// <summary>
    /// Default store constructor.
    /// </summary>
    public Store(string identifier, IStoreDefinition storeDefinition)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException("An identifier is missing for the store");
        }

        if (storeDefinition is null)
        {
            throw new ArgumentException("The store definition is missing.");
        }

        // Set store identifier
        Identifier = identifier;

        // Set store definition
        _storeDefinition = storeDefinition;

        // Register all node subscriptions
        ICollection<string> nodes = _storeDefinition.GetNodes();

        _storeSubscriber = new StoreSubscriber(nodes);
        _storeSubscriber.HandleChangeNode += OnHandleChangeNode;

        foreach (string node in nodes)
        {
            _storeSubscriber.RegisterNode(node);
        }

        // Init store data
        Data = _storeDefinition.CreateDataInstance();
    }

    /// <summary>
    /// Connect the node subscriptions to the store.
    /// </summary>
    /// <param name="nodeSubscriptions">Node subscriptions</param>
    public void ConnectToStore(ICollection<NodeSubscription> nodeSubscriptions)
    {
        if (nodeSubscriptions is null)
        {
            return;
        }

        foreach (NodeSubscription subscription in nodeSubscriptions)
        {
            _storeSubscriber.SubscribeToNode(subscription);
        }
    }

    /// <summary>
    /// Disconnect the node subscriptions to the store.
    /// </summary>
    /// <param name="nodeSubscriptions">Node subscriptions</param>
    public void DisconnectToStore(ICollection<NodeSubscription> nodeSubscriptions)
    {
        if (nodeSubscriptions is null)
        {
            return;
        }

        foreach (NodeSubscription subscription in nodeSubscriptions)
        {
            _storeSubscriber.UnsubscribeToNode(subscription);
        }
    }

    /// <summary>
    /// Dispatch the data. 
    /// </summary>
    /// <param name="action">Action</param>
    public async Task DispatchAsync(IAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_dataLock)
        {
            _storeDefinition.SetValue(Data, action.Node, action.Data);
        }

        await _storeSubscriber.EmitNodeChangeAsync(action.Node);
    }

    /// <summary>
    /// Get the node value in the store.
    /// </summary>
    /// <param name="node">Node name</param>
    /// <returns>Value of the node</returns>
    public object? GetNodeValue(string node) 
        => _storeDefinition.GetValue(Data, node);

    /// <summary>
    /// Dispose the store.
    /// </summary>
    public void Dispose()
    {
        _storeSubscriber.HandleChangeNode -= OnHandleChangeNode;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Call when the node value is changed.
    /// </summary>
    /// <param name="node">Node name</param>
    protected virtual async Task OnHandleChangeNode(string node)
    {
        Func<Task> handleChangeFunc = _storeSubscriber.GetNodeHandleChange(node);
        await handleChangeFunc();
    }
}


/// <summary>
/// Store
/// </summary>
public class Store<TStore> : IStore, IStoreManager<TStore>, IDisposable where TStore : new()
{
    /// <summary>
    /// Data of the store.
    /// </summary>
    public TStore Data { get; private set; }

    /// <summary>
    /// Identifier
    /// </summary>
    public string Identifier { get; private set; }

    /// <summary>
    /// Store definition
    /// </summary>
    private readonly IStoreDefinition<TStore> _storeDefinition;

    /// <summary>
    /// Store subscriber.
    /// </summary>
    private readonly StoreSubscriber _storeSubscriber;

    /// <summary>
    /// Lock for the data
    /// </summary>
    private readonly object _dataLock = new();

    /// <summary>
    /// Store constructor for TypedStoreDefinition
    /// </summary>
    /// <param name="identifier">Store identifier</param>
    public Store(string identifier) : this(identifier, new TypedStoreDefinition<TStore>())
    {
    }

    /// <summary>
    /// Default store constructor.
    /// </summary>
    public Store(string identifier, IStoreDefinition<TStore> storeDefinition)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException("An identifier is missing for the store");
        }

        if (storeDefinition is null)
        {
            throw new ArgumentException("The store definition is missing.");
        }

        // Set store identifier
        Identifier = identifier;

        // Set store definition
        _storeDefinition = storeDefinition;

        // Register all node subscriptions
        ICollection<string> nodes = _storeDefinition.GetNodes();

        _storeSubscriber = new StoreSubscriber(nodes);
        _storeSubscriber.HandleChangeNode += OnHandleChangeNode;

        foreach (string node in nodes)
        {
            _storeSubscriber.RegisterNode(node);
        }

        // Init store data
        Data = _storeDefinition.CreateDataInstance() 
            ?? throw new InvalidOperationException("Store must be instanciated");
    }

    /// <summary>
    /// Connect the node subscriptions to the store.
    /// </summary>
    /// <param name="nodeSubscriptions">Node subscriptions</param>
    public void ConnectToStore(ICollection<NodeSubscription> nodeSubscriptions)
    {
        if (nodeSubscriptions is null)
        {
            return;
        }

        foreach (NodeSubscription subscription in nodeSubscriptions)
        {
            _storeSubscriber.SubscribeToNode(subscription);
        }
    }

    /// <summary>
    /// Disconnect the node subscriptions to the store.
    /// </summary>
    /// <param name="nodeSubscriptions">Node subscriptions</param>
    public void DisconnectToStore(ICollection<NodeSubscription> nodeSubscriptions)
    {
        if (nodeSubscriptions is null)
        {
            return;
        }

        foreach (NodeSubscription subscription in nodeSubscriptions)
        {
            _storeSubscriber.UnsubscribeToNode(subscription);
        }
    }

    /// <summary>
    /// Dispatch the data. 
    /// </summary>
    /// <param name="action">Action</param>
    public async Task DispatchAsync<TNode>(IAction<TNode> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_dataLock)
        {
            _storeDefinition.SetValue(Data, action.Node, action.Data);
        }

        await _storeSubscriber.EmitNodeChangeAsync(action.Node);
    }

    /// <summary>
    /// Get the node value in the store.
    /// </summary>
    /// <param name="node">Node name</param>
    /// <returns>Value of the node</returns>
    public TNode? GetNodeValue<TNode>(string node) 
        => _storeDefinition.GetValue<TNode>(Data, node);

    /// <summary>
    /// Dispose the store.
    /// </summary>
    public void Dispose()
    {
        _storeSubscriber.HandleChangeNode -= OnHandleChangeNode;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Call when the node value is changed.
    /// </summary>
    /// <param name="node">Node name</param>
    protected virtual async Task OnHandleChangeNode(string node)
    {
        Func<Task> handleChangeFunc = _storeSubscriber.GetNodeHandleChange(node);
        await handleChangeFunc();
    }
}
