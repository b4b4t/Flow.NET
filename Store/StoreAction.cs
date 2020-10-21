using Flow.Store.Contracts;

namespace Flow.Store
{
    public class StoreAction : IAction
    {
        public StoreAction(string identifier, string node, object data)
        {
            Identifier = identifier;
            Node = node;
            Data = data;
        }

        public string Identifier { get; set; }
        public string Node { get; set; }
        public object Data { get; set; }
    }
}
