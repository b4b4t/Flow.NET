﻿namespace Flow.Store.Contracts;

/// <summary>
/// Action
/// </summary>
public interface IAction
{
    /// <summary>
    /// Store identifier
    /// </summary>
    string Identifier { get; set; }

    /// <summary>
    /// Node name
    /// </summary>
    string Node { get; set; }

    /// <summary>
    /// Data
    /// </summary>
    object Data { get; set; }
}

/// <summary>
/// Action
/// </summary>
public interface IAction<TNode>
{
    /// <summary>
    /// Store identifier
    /// </summary>
    string Identifier { get; set; }

    /// <summary>
    /// Node name
    /// </summary>
    string Node { get; set; }

    /// <summary>
    /// Data
    /// </summary>
    TNode Data { get; set; }
}
