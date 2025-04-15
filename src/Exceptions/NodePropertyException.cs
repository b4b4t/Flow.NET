using System;

namespace Flow.Exceptions;

/// <summary>
/// Exception on a node property in a store
/// </summary>
public class NodePropertyException(string message) : Exception(message)
{
}
