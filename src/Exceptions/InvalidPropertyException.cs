using System;

namespace Flow.Exceptions;

/// <summary>
/// Represents errors that occur when a setter of a node property is missing, read-only or public
/// </summary>
public class InvalidPropertyException(string storeDefinition, string node, bool hasPublicSetter)
    : Exception(GetExceptionMessage(storeDefinition, node, hasPublicSetter))
{
    /// <summary>
    /// Get the exception message.
    /// </summary>
    /// <param name="storeDefinition">Store definition name</param>
    /// <param name="node">Identifier of the node property</param>
    /// <param name="hasPublicSetter">If the property has a public setter</param>
    /// <returns></returns>
    private static string GetExceptionMessage(string storeDefinition, string node, bool hasPublicSetter) 
        => hasPublicSetter ? $"{storeDefinition}.{node} must have a non public setter" : $"{storeDefinition}.{node} is read-only or there is no set accessor";
}
