using Flow.Contracts;
using Flow.Subscription;
using System;
using System.Collections.Generic;

namespace Flow.Store
{
    public class StoreContainer
    {
        public Dictionary<string, IStore> Stores { get; private set; } = new Dictionary<string, IStore>();

        public void Register(string identifier, IStore store)
        {
            Stores.Add(identifier, store);

            ReadStores();
        }

        public void Disptach(string identifier, string node, object data)
        {
            IStore store = GetStore(identifier);

            store.Dispatch(node, data);
        }

        public void ConnectToStores(ICollection<StoreSubscription> storeSubscriptions)
        {
            if (storeSubscriptions is null)
            {
                throw new ArgumentNullException(nameof(storeSubscriptions));
            }

            foreach (var storeSubscription in storeSubscriptions)
            {
                IStore store = GetStore(storeSubscription.Identifier);

                store.ConnectToStore(storeSubscription.NodeSubscriptions);
            }
        }

        public void DisconnectToStores(ICollection<StoreSubscription> storeSubscriptions)
        {
            if (storeSubscriptions is null)
            {
                throw new ArgumentNullException(nameof(storeSubscriptions));
            }

            foreach (var storeSubscription in storeSubscriptions)
            {
                IStore store = GetStore(storeSubscription.Identifier);

                store.DisconnectToStore(storeSubscription.NodeSubscriptions);
            }
        }

        public IStore GetStore(string identifier)
        {
            if (!Stores.ContainsKey(identifier))
            {
                throw new ArgumentException($"No store found with this identifier : {identifier}");
            }

            return Stores[identifier];
        }

        private void ReadStores()
        {
            foreach(string identifier in Stores.Keys)
            {
                Console.WriteLine(identifier);
            }
        }
    }
}
