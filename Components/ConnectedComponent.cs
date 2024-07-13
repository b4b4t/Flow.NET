using Flow.Store;
using Flow.Store.Contracts;
using Flow.Subscription;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Flow.Components;

/// <summary>
/// Component connected to the stores
/// </summary>
public class ConnectedComponent : ComponentBase, IDisposable
{
    [Inject]
    protected StoreContainer StoreContainer { get; set; }

    [Inject]
    private ILogger<ConnectedComponent> Logger { get; set; }

    /// <summary>
    /// Component type
    /// </summary>
    private Type _componentType;

    /// <summary>
    /// Store subscriptions
    /// </summary>
    private ICollection<StoreSubscription> _storeSubscriptions;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        SetComponentType();
        ConnectNodesToStores();
        SetPropertiesDataFromStores();
    }

    /// <summary>
    /// Set the component type
    /// </summary>
    private void SetComponentType() => _componentType = GetType();

    /// <summary>
    /// Connect the properties to the store nodes.
    /// </summary>
    private void ConnectNodesToStores()
    {
        // Get the component properties
        PropertyInfo[] properties = _componentType.GetProperties();

        // Add the node subscription for each property having a Store connector attribute
        Dictionary<string, StoreSubscription> storeSubscriptions = [];

        foreach (PropertyInfo propertyInfo in properties)
        {
            StoreConnectorAttribute storeConnector = propertyInfo.GetCustomAttribute<StoreConnectorAttribute>();

            if (storeConnector is not null)
            {
                StoreSubscription storeSubscription = CreateOrGetStoreSubscription(storeSubscriptions, storeConnector.Identifier);
                NodeSubscription nodeSubscription = CreateNodeSubscription(storeConnector, propertyInfo);

                storeSubscription.AddNodeSubription(nodeSubscription);
            }
        }

        _storeSubscriptions = storeSubscriptions.Values;

        StoreContainer.ConnectToStores(_storeSubscriptions);
    }

    /// <summary>
    /// Set the values of the properties with the data from the stores.
    /// </summary>
    protected virtual void SetPropertiesDataFromStores()
    {
        PropertyInfo[] properties = _componentType.GetProperties();

        foreach (PropertyInfo propertyInfo in properties)
        {
            StoreConnectorAttribute storeConnector = propertyInfo.GetCustomAttribute<StoreConnectorAttribute>();

            if (storeConnector is not null)
            {
                IStore store = StoreContainer.GetStore(storeConnector.Identifier);
                object data = store.GetNodeValue(storeConnector.NodeName);

                propertyInfo.SetValue(this, data);

                Logger.LogInformation("Store {Identifier} -> Node {NodeName} = {data}", storeConnector.Identifier, storeConnector.NodeName, data);
            }
        }
    }

    /// <summary>
    /// Create or get a store subscription.
    /// </summary>
    /// <param name="storeSubscriptions">Existing store subscriptions by identifier</param>
    /// <param name="identifier">Store identifier</param>
    /// <returns>Store susbcription</returns>
    private static StoreSubscription CreateOrGetStoreSubscription(Dictionary<string, StoreSubscription> storeSubscriptions, string identifier)
    {
        if (storeSubscriptions.TryGetValue(identifier, out StoreSubscription value))
        {
            return value;
        }
        else
        {
            StoreSubscription storeSubscription = new (identifier);

            storeSubscriptions.Add(identifier, storeSubscription);

            return storeSubscription;
        }
    }

    /// <summary>
    /// Create a node subscription.
    /// </summary>
    /// <param name="storeConnector">Store connector</param>
    /// <param name="propertyInfo">Property info</param>
    /// <returns>Node subscription</returns>
    private NodeSubscription CreateNodeSubscription(StoreConnectorAttribute storeConnector, PropertyInfo propertyInfo)
    {
        string storeName = storeConnector.Identifier;

        if (!StoreContainer.Stores.ContainsKey(storeName))
        {
            throw new Exception($"Can't find the store : {storeName}");
        }

        Logger.LogInformation("CreateNodeSubscription : {NodeName}", storeConnector.NodeName);

        Action handleChangeAction = HandleChangeFactory(storeConnector, propertyInfo);

        return new NodeSubscription(storeConnector.NodeName, handleChangeAction);
    }

    /// <summary>
    /// Create a handle change action for a store and a node.
    /// </summary>
    /// <param name="storeConnector">Store connector</param>
    /// <param name="property">Property info</param>
    /// <returns>Action</returns>
    protected virtual Action HandleChangeFactory(StoreConnectorAttribute storeConnector, PropertyInfo property)
    {
        Logger.LogInformation("HandleChangeFactory : {Name}", property.Name);

        void handleChangeAction()
        {
            Logger.LogInformation("Component handle change : {NodeName}", storeConnector.NodeName);

            Type type = property.PropertyType;
            IStore store = StoreContainer.Stores[storeConnector.Identifier];
            object data = Convert.ChangeType(store.GetNodeValue(storeConnector.NodeName), type);

            Logger.LogInformation("Set node value : {data}", data);

            property.SetValue(this, data);

            OnNodeChanged(storeConnector.NodeName, property.Name);
            StateHasChanged();
        }

        return handleChangeAction;
    }

    /// <summary>
    /// Call when a node changed.
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="propertyName">Node property name in the component</param>
    protected virtual void OnNodeChanged(string nodeName, string propertyName)
    {

    }

    /// <summary>
    /// Dispatch data.
    /// </summary>
    /// <param name="action">Action</param>
    protected void Dispatch(IAction action) => StoreContainer.Disptach(action);

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        StoreContainer.DisconnectToStores(_storeSubscriptions);
        GC.SuppressFinalize(this);
    }
}
