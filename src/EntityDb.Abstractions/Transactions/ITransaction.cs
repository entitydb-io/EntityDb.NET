using EntityDb.Abstractions.Transactions.Steps;
using System;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Transactions;

/// <summary>
///     Represents a set of objects which must be committed together or not at all. Possible objects include: agentSignatures,
///     commands, leases, and tags.
/// </summary>
public interface ITransaction
{
    /// <summary>
    ///     The id associated with the set of objects.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     The date and time associated with the set of objects.
    /// </summary>
    DateTime TimeStamp { get; }

    /// <summary>
    ///     The signature of the agent who requested this transaction.
    /// </summary>
    object AgentSignature { get; }

    /// <summary>
    ///     A series of sets of modifiers for a set of entities.
    /// </summary>
    /// <remarks>
    ///     <see cref="Steps" /> must be handled in the order they are given.
    /// </remarks>
    ImmutableArray<ITransactionStep> Steps { get; }
}
