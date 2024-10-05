using Flow.Exceptions;
using Flow.Store.Contracts;
using Flow.Subscription;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

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
    public void Disptach(IAction action)
    {
        IStore store = GetStore(action.Identifier);

        store.Dispatch(action);
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
        if (!Stores.TryGetValue(identifier, out IStore value))
        {
            throw new MissingStoreException(identifier);
        }

        return value;
    }

    /// <summary>
    /// Read the stores identifiers
    /// </summary>
    private void ReadStores()
    {
        foreach(string identifier in Stores.Keys)
        {
            _logger.LogInformation("Store : {identifier}", identifier);
        }
    }
}
