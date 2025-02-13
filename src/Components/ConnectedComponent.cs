using Flow.Store;
using Flow.Store.Contracts;
using Flow.Subscription;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Flow.Components;

/// <summary>
/// Component connected to the stores
/// </summary>
public class ConnectedComponent : ComponentBase, IDisposable
{
    /// <summary>
    /// Store container
    /// </summary>
    [Inject]
    protected StoreContainer StoreContainer { get; set; } = null!;

    /// <summary>
    /// Logger
    /// </summary>
    [Inject]
    protected ILogger<ConnectedComponent> Logger { get; set; } = null!;

    /// <summary>
    /// Component type
    /// </summary>
    private Type? _componentType;

    /// <summary>
    /// Store subscriptions
    /// </summary>
    private ICollection<StoreSubscription>? _storeSubscriptions;

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
        PropertyInfo[] properties = _componentType?.GetProperties() ?? throw new InvalidOperationException("Cannot get the component type");

        // Add the node subscription for each property having a Store connector attribute
        Dictionary<string, StoreSubscription> storeSubscriptions = [];

        foreach (PropertyInfo propertyInfo in properties)
        {
            StoreConnectorAttribute? storeConnector = propertyInfo.GetCustomAttribute<StoreConnectorAttribute>();

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
        PropertyInfo[] properties = _componentType?.GetProperties() ?? throw new InvalidOperationException("Cannot get the component type"); ;

        foreach (PropertyInfo propertyInfo in properties)
        {
            StoreConnectorAttribute? storeConnector = propertyInfo.GetCustomAttribute<StoreConnectorAttribute>();

            if (storeConnector is not null)
            {
                IStore store = StoreContainer.GetStore(storeConnector.Identifier);
                object? data;
                if (store is IStoreManager storeManager)
                {
                    data = storeManager.GetNodeValue(storeConnector.NodeName);
                }
                else if (IsGenericStoreManager(store, out Type? storeManagerType))
                {
                    data = ExecuteGetNodeValue(storeManagerType!, propertyInfo.PropertyType, store, storeConnector.NodeName);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Store node (Store {storeConnector.Identifier} -> Node {storeConnector.NodeName}) cannot be fetched for the property {propertyInfo.Name}");
                }
                
                propertyInfo.SetValue(this, data);

                Logger.LogDebug("Fetched property {Name} : Store {Identifier} -> Node {NodeName}", 
                    propertyInfo.Name, storeConnector.Identifier, storeConnector.NodeName);
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
        if (storeSubscriptions.TryGetValue(identifier, out StoreSubscription? value))
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

        Logger.LogDebug("CreateNodeSubscription : {NodeName} -> Property : {PropertyName}", storeConnector.NodeName, propertyInfo.Name);

        Func<Task> handleChangeFunc = HandleChangeFactory(storeConnector, propertyInfo);

        return new NodeSubscription(storeConnector.NodeName, handleChangeFunc);
    }

    /// <summary>
    /// Create a handle change action for a store and a node.
    /// </summary>
    /// <param name="storeConnector">Store connector</param>
    /// <param name="property">Property info</param>
    /// <returns>Action</returns>
    protected virtual Func<Task> HandleChangeFactory(StoreConnectorAttribute storeConnector, PropertyInfo property)
    {
        Logger.LogDebug("HandleChangeFactory : {Name} -> Property : {PropertyName}", property.Name, property.Name);

        async Task handleChangeActionAsync()
        {
            Type type = property.PropertyType;
            IStore store = StoreContainer.Stores[storeConnector.Identifier];
            object? data;

            if (store is IStoreManager storeManager)
            {
                data = Convert.ChangeType(storeManager.GetNodeValue(storeConnector.NodeName), type);
            }
            else if (IsGenericStoreManager(store, out Type? storeManagerType))
            {
                data = ExecuteGetNodeValue(storeManagerType!, type, store, storeConnector.NodeName);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Property {property.Name} cannot be updated for store node (Store {storeConnector.Identifier} -> Node {storeConnector.NodeName})");
            }

            property.SetValue(this, data);

            Logger.LogInformation("Property {Name} updated with store node (Store {Identifier} -> Node {NodeName})",
                        property.Name, storeConnector.Identifier, storeConnector.NodeName);

            OnNodeChanged(storeConnector.NodeName, property.Name);

            await InvokeAsync(StateHasChanged);
        }

        return handleChangeActionAsync;
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
    protected async Task DispatchAsync(IAction action) => await StoreContainer.DispatchAsync(action);

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        if (_storeSubscriptions is not null)
        {
            StoreContainer.DisconnectToStores(_storeSubscriptions);
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Check if the store is a generic store manager.
    /// </summary>
    /// <param name="store">Store</param>
    /// <returns>True if the store is a generic store manager.</returns>
    private static bool IsGenericStoreManager(IStore store, out Type? storeManagerType)
    {
        Type storeType = store.GetType();

        if (storeType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStoreManager<>)))
        {
            storeManagerType = storeType;
            return true;
        }
        else
        {
            storeManagerType = null;
            return false;
        }
    }

    /// <summary>
    /// Execute the GetNodeValue method of the generic store manager.
    /// </summary>
    /// <param name="storeManagerType">Store manager type</param>
    /// <param name="store">Store</param>
    /// <param name="node">Node name</param>
    /// <returns></returns>
    private static object? ExecuteGetNodeValue(Type storeManagerType, Type nodeType, object store, string node)
    {
        // Get the method info for GetNodeValue
        MethodInfo method = storeManagerType.GetMethod(nameof(IStoreManager<object>.GetNodeValue))?.MakeGenericMethod(nodeType)
            ?? throw new InvalidOperationException($"{nameof(IStoreManager<object>.GetNodeValue)} method not found.");

        return method.Invoke(store, [node]);
    }
}
