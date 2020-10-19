using Flow.Contracts;
using Flow.Subscription;
using System;
using System.Collections.Generic;

namespace Flow.Store
{
    public class Store : IStore, IDisposable
    {
        /// <summary>
        /// Data of the store.
        /// </summary>
        public object Data { get; }

        private readonly IStoreDefinition _storeDefinition;

        /// <summary>
        /// Store subscriber.
        /// </summary>
        private readonly StoreSubscriber _storeSubscriber;

        /// <summary>
        /// Default store constructor.
        /// </summary>
        public Store()
        {
            _storeSubscriber = new StoreSubscriber();
        }

        public Store(IStoreDefinition storeDefinition)
        {
            if (storeDefinition is null)
            {
                throw new Exception("The store definition is missing.");
            }

            _storeDefinition = storeDefinition;

            ICollection<string> nodes = _storeDefinition.GetNodes();

            _storeSubscriber = new StoreSubscriber(nodes);
            _storeSubscriber.HandleChangeNode += OnHandleChangeNode;

            foreach (string node in nodes)
            {
                _storeSubscriber.RegisterNode(node);
            }

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
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="node">Node name</param>
        /// <param name="data">Data</param>
        public void Dispatch<T>(string node, T data)
        {
            lock (Data)
            {
                _storeDefinition.SetValue(Data, node, data);
            }

            _storeSubscriber.EmitNodeChange(node);
        }

        /// <summary>
        /// Get the node value in the store.
        /// </summary>
        /// <param name="node">Node name</param>
        /// <returns>Value of the node</returns>
        public object GetNodeValue(string node)
        {
            return _storeDefinition.GetValue(Data, node);
        }

        /// <summary>
        /// Dispose the store.
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("Dispose");

            _storeSubscriber.HandleChangeNode -= OnHandleChangeNode;
        }

        /// <summary>
        /// Call when the node value is changed.
        /// </summary>
        /// <param name="node">Node name</param>
        protected virtual void OnHandleChangeNode(string node)
        {
            Console.WriteLine($"OnHandleChangeNode : {node}");

            Action handleChangeAction = _storeSubscriber.GetNodeHandleChange(node);

            handleChangeAction?.DynamicInvoke();
        }
    }
}
