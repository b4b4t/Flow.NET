using Flow.Subscription;
using System.Collections.Generic;

namespace Flow.Store.Contracts
{
    public interface IStore
    {
        object GetNodeValue(string node);

        void ConnectToStore(ICollection<NodeSubscription> nodeSubscriptions);
        
        void DisconnectToStore(ICollection<NodeSubscription> nodeSubscriptions);

        void Dispatch(IAction action);
    }
}
