using System;
using System.Runtime.CompilerServices;

namespace Flow.Store;

/// <summary>
/// Attribute used to connect a property to a store node
/// </summary>
/// <param name="storeName">Store identifier</param>
/// <param name="nodeName">Node name</param>
/// <param name="propertyName">Property name</param>
[AttributeUsage(AttributeTargets.Property)]
public class StoreConnectorAttribute(
    string storeName,
    string nodeName = null,
    [CallerMemberName] string propertyName = null
    ) : Attribute
{
    /// <summary>
    /// Store identifier
    /// </summary>
    public string Identifier { get; set; } = storeName;
    
    /// <summary>
    /// Node name
    /// </summary>
    public string NodeName { get; set; } = nodeName ?? propertyName;
    
    /// <summary>
    /// Property name
    /// </summary>
    public string PropertyName { get; set; } = propertyName;
}
