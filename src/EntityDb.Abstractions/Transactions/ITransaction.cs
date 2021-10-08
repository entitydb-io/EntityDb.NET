using System;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    ///     Represents a set of objects which must be committed together or not at all. Possible objects include: sources,
    ///     commands, leases, and tags.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities to be modified.</typeparam>
    public interface ITransaction<TEntity>
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
        ///     A description of the agent who has requested this transaction.
        /// </summary>
        object Source { get; }

        /// <summary>
        ///     A series of sets of modifiers for a set of entities.
        /// </summary>
        /// <remarks>
        ///     <see cref="Steps" /> must be handled in the order they are given.
        /// </remarks>
        ImmutableArray<ITransactionStep<TEntity>> Steps { get; }
    }
}
