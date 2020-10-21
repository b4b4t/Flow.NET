using Flow.Store;
using Flow.Store.Contracts;
using Flow.Subscription;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Flow.Components
{
    public class ConnectedComponent : ComponentBase, IDisposable
    {
        [Inject]
        protected StoreContainer StoreContainer { get; set; }

        private Type _componentType;

        private ICollection<StoreSubscription> _storeSubscriptions;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            SetComponentType();
            ConnectNodesToStores();
            SetPropertiesDataFromStores();
        }

        private void SetComponentType()
        {
            _componentType = this.GetType();
        }

        /// <summary>
        /// Connect the properties to the store nodes.
        /// </summary>
        private void ConnectNodesToStores()
        {
            PropertyInfo[] properties = _componentType.GetProperties();
            Dictionary<string, StoreSubscription> storeSubscriptions = new Dictionary<string, StoreSubscription>();

            foreach (PropertyInfo propertyInfo in properties)
            {
                StoreConnectorAttribute storeConnector = propertyInfo.GetCustomAttribute<StoreConnectorAttribute>();

                if (storeConnector != null)
                {
                    var storeSubscription = CreateOrGetStoreSubscription(storeSubscriptions, storeConnector.Identifier);
                    var nodeSubscription = CreateNodeSubscription(storeConnector, propertyInfo);

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

                if (storeConnector != null)
                {
                    IStore store = StoreContainer.GetStore(storeConnector.Identifier);
                    object data = store.GetNodeValue(storeConnector.NodeName);

                    propertyInfo.SetValue(this, data);

                    Console.WriteLine($"Store {storeConnector.Identifier} -> Node {storeConnector.NodeName} = {data}");
                }
            }
        }

        /// <summary>
        /// Create or get a store subscription.
        /// </summary>
        /// <param name="storeSubscriptions">Existing store subscriptions by identifier</param>
        /// <param name="identifier">Store identifier</param>
        /// <returns>Store susbcription</returns>
        private StoreSubscription CreateOrGetStoreSubscription(Dictionary<string, StoreSubscription> storeSubscriptions, string identifier)
        {
            if (storeSubscriptions.ContainsKey(identifier))
            {
                return storeSubscriptions[identifier];
            }
            else
            {
                StoreSubscription storeSubscription = new StoreSubscription(identifier);

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

            Console.WriteLine($"CreateNodeSubscription : {storeConnector.NodeName}");

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
            Console.WriteLine($"HandleChangeFactory : {property.Name}");

            Action handleChangeAction = () =>
            {
                Console.WriteLine($"Component handle change : {storeConnector.NodeName}");

                Type type = property.PropertyType;
                IStore store = StoreContainer.Stores[storeConnector.Identifier];
                object data = Convert.ChangeType(store.GetNodeValue(storeConnector.NodeName), type);

                Console.WriteLine($"Set node value : {data}");
                property.SetValue(this, data);

                StateHasChanged();
            };

            return handleChangeAction;
        }

        /// <summary>
        /// Dispatch data.
        /// </summary>
        /// <param name="identifier">Store identifier</param>
        /// <param name="node">Node name</param>
        /// <param name="data">Data</param>
        protected void Dispatch(string identifier, string node, object data)
        {
            StoreContainer.Disptach(identifier, node, data);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            StoreContainer.DisconnectToStores(_storeSubscriptions);
        }
    }
}
