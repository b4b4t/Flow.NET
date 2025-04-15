using Flow.Exceptions;
using Flow.Store.Contracts;
using Flow.Subscription;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flow.Store;

/// <summary>
/// Store container
/// </summary>
public class StoreContainer(ILogger<StoreContainer> logger)
{
    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<StoreContainer> _logger = logger;

    /// <summary>
    /// List of stores
    /// </summary>
    public Dictionary<string, IStore> Stores { get; private set; } = [];

    /// <summary>
    /// Register a store
    /// </summary>
    /// <param name="store">Store</param>
    public void Register(IStore store)
    {
        Stores.Add(store.Identifier, store);

        ReadStores();
    }

    /// <summary>
    /// Dispatch an action
    /// </summary>
    /// <param name="action">Action</param>
    public async Task DispatchAsync(IAction action)
    {
        IStoreManager storeManager = GetStoreManager(action.Identifier);

        await storeManager.DispatchAsync(action);
    }

    /// <summary>
    /// Dispatch an action
    /// </summary>
    /// <param name="action">Action</param>
    public async Task DispatchAsync<TStore>(IAction<TStore> action)
    {
        IStoreManager<TStore> storeManager = GetStoreManager<TStore>(action.Identifier);

        await storeManager.DispatchAsync(action);
    }

    /// <summary>
    /// Connect subscritptions to stores
    /// </summary>
    /// <param name="storeSubscriptions">Store subscriptions</param>
    public void ConnectToStores(ICollection<StoreSubscription> storeSubscriptions)
    {
        ArgumentNullException.ThrowIfNull(storeSubscriptions);

        foreach (var storeSubscription in storeSubscriptions)
        {
            IStore store = GetStore(storeSubscription.Identifier);

            store.ConnectToStore(storeSubscription.NodeSubscriptions);
        }
    }

    /// <summary>
    /// Disconnect subscriptions to stores
    /// </summary>
    /// <param name="storeSubscriptions">Store subscriptions</param>
    /// <exception cref="ArgumentNullException">If storeSubscriptions is null</exception>
    public void DisconnectToStores(ICollection<StoreSubscription> storeSubscriptions)
    {
        ArgumentNullException.ThrowIfNull(storeSubscriptions);

        foreach (var storeSubscription in storeSubscriptions)
        {
            IStore store = GetStore(storeSubscription.Identifier);

            store.DisconnectToStore(storeSubscription.NodeSubscriptions);
        }
    }

    /// <summary>
    /// Get a store from an identifier
    /// </summary>
    /// <param name="identifier">Store identifier</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If </exception>
    public IStore GetStore(string identifier)
    {
        if (!Stores.TryGetValue(identifier, out IStore? value))
        {
            throw new MissingStoreException(identifier);
        }

        return value;
    }    
    
    /// <summary>
    /// Get a store manager from an identifier
    /// </summary>
    /// <param name="identifier">Store identifier</param>
    /// <returns>Store manager</returns>
    public IStoreManager GetStoreManager(string identifier)
    {
        IStore store = GetStore(identifier);

        if (store is IStoreManager storeManager)
        {
            return storeManager;
        }

        throw new InvalidOperationException($"The store manager is not a {nameof(IStoreManager)}");
    }    

    /// <summary>
    /// Get a store manager from an identifier
    /// </summary>
    /// <param name="identifier">Store identifier</param>
    /// <returns>Store manager</returns>
    public IStoreManager<TStore> GetStoreManager<TStore>(string identifier)
    {
        IStore store = GetStore(identifier);

        if (store is IStoreManager<TStore> storeManager)
        {
            return storeManager;
        }

        throw new InvalidOperationException($"The store manager is not a {nameof(IStoreManager<TStore>)}");
    }

    /// <summary>
    /// Read the stores identifiers
    /// </summary>
    private void ReadStores() 
        => _logger.LogInformation("Stores : {identifier}", string.Join(", ", Stores.Keys));
}
