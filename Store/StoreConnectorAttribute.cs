using System;
using System.Runtime.CompilerServices;

namespace Flow.Store
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StoreConnectorAttribute : Attribute
    {
        public string Identifier { get; set; }
        public string NodeName { get; set; }
        public string PropertyName { get; set; }

        public StoreConnectorAttribute(
            string storeName, 
            string nodeName = null, 
            [CallerMemberName] string propertyName = null
        ) {
            Identifier = storeName;
            NodeName = nodeName ?? propertyName;
            PropertyName = propertyName;
        }
    }
}
