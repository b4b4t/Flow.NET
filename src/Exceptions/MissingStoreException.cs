using System;

namespace Flow.Exceptions;

/// <summary>
/// Represents errors that occur when a store is missing in the store container
/// </summary>
public class MissingStoreException(string identifier)
    : Exception($"No store found with this identifier : {identifier}")
{
}
