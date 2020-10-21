using Flow.Subscription;
using System.Collections.Generic;

namespace Flow.Store.Contracts
{
    public interface IStore
    {
        void Dispatch<T>(string node, T data);

        object GetNodeValue(string node);

        void ConnectToStore(ICollection<NodeSubscription> nodeSubscriptions);
        
        void DisconnectToStore(ICollection<NodeSubscription> nodeSubscriptions);
    }
}
