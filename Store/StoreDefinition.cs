using System;
using System.Collections.Generic;
using System.Reflection;

namespace Flow.Store
{
    public class StoreDefinition<T> : IStoreDefinition where T: class, new()
    {
        private Dictionary<string, PropertyInfo> _nodeProperties;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StoreDefinition()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            _nodeProperties = new Dictionary<string, PropertyInfo>(properties.Length);

            foreach (var property in properties)
            {
                _nodeProperties.Add(property.Name, property);
            }
        }

        /// <inheritdoc cref="IStoreDefinition.CreateDataInstance"/>
        public object CreateDataInstance()
        {
            return new T();
        }

        /// <inheritdoc cref="IStoreDefinition.GetNodes"/>
        public ICollection<string> GetNodes()
        {
            return _nodeProperties.Keys;
        }

        /// <inheritdoc cref="IStoreDefinition.GetValue(object, string)"/>
        public object GetValue(object data, string node)
        {
            CheckNode(node);

            return _nodeProperties[node].GetValue(data);
        }

        /// <inheritdoc cref="IStoreDefinition.SetValue(object, string, object)"/>
        public void SetValue(object data, string node, object value)
        {
            CheckNode(node);

            _nodeProperties[node].SetValue(data, value);
        }

        /// <summary>
        /// Check if the node exists in the store definition.
        /// </summary>
        /// <param name="node">Node name</param>
        private void CheckNode(string node)
        {
            if (!_nodeProperties.ContainsKey(node))
            {
                throw new ArgumentException($"Node {node} does not exist");
            }
        }
    }
}
