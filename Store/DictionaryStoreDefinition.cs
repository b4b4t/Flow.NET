using Flow.Store.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Store
{
    public class DictionaryStoreDefinition : IStoreDefinition
    {
        /// <summary>
        /// Node list.
        /// </summary>
        public string[] _nodes;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="nodes"></param>
        public DictionaryStoreDefinition(string[] nodes)
        {
            _nodes = nodes;
        }

        /// <inheritdoc cref="IStoreDefinition.CreateDataInstance"/>
        public object CreateDataInstance()
        {
            Dictionary<string, object> data = new Dictionary<string, object>(_nodes.Length);

            foreach (string node in _nodes)
            {
                data.Add(node, null);
            }

            return data;
        }

        /// <inheritdoc cref="IStoreDefinition.GetNodes"/>
        public ICollection<string> GetNodes()
        {
            return _nodes;
        }

        /// <inheritdoc cref="IStoreDefinition.GetValue(object, string)"/>
        public object GetValue(object data, string node)
        {
            CheckNode(node);

            var storeData = data as IDictionary<string, object>;

            return storeData[node];
        }

        /// <inheritdoc cref="IStoreDefinition.SetValue(object, string, object)"/>
        public void SetValue(object data, string node, object value)
        {
            CheckNode(node);

            var storeData = data as IDictionary<string, object>;

            storeData[node] = value;
        }

        /// <summary>
        /// Check if the node exists in the store definition.
        /// </summary>
        /// <param name="node">Node name</param>
        private void CheckNode(string node)
        {
            if (!_nodes.Contains(node))
            {
                throw new ArgumentException($"Node {node} does not exist");
            }
        }
    }
}
