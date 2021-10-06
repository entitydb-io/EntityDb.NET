using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using System;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    ///     Represents a set of modifiers for a single entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be modified.</typeparam>
    public interface ITransactionCommand<TEntity>
    {
        /// <summary>
        ///     A snapshot of the entity after the command.
        /// </summary>
        TEntity NextSnapshot { get; }

        /// <summary>
        ///     The id of the entity.
        /// </summary>
        Guid EntityId { get; }

        /// <summary>
        ///     The previous version number of the entity.
        /// </summary>
        /// <remarks>
        ///     The repository must use a VersionNumber equal to <see cref="ExpectedPreviousVersionNumber" /> + 1.
        /// </remarks>
        ulong ExpectedPreviousVersionNumber { get; }

        /// <summary>
        ///     The intent.
        /// </summary>
        ICommand<TEntity> Command { get; }

        /// <summary>
        ///     The set of modifiers.
        /// </summary>
        /// <remarks>
        ///     <see cref="Facts" /> does not need to be ordered, but each entry must have a unique
        ///     <see cref="ITransactionFact{TEntity}.SubversionNumber" /> for the given
        ///     <see cref="ExpectedPreviousVersionNumber" />.
        ///     All though it may seem awkward, this provides two benefits.
        ///     1. None of the repository implementations need to implement subversion number counting.
        ///     2. The generic test suite can verify that the uniqueness constraint is satisfied.
        /// </remarks>
        ImmutableArray<ITransactionFact<TEntity>> Facts { get; }

        /// <summary>
        ///     The unique metadata properties.
        /// </summary>
        ITransactionMetaData<ILease> Leases { get; }

        /// <summary>
        ///     The non-unique metadata properties.
        /// </summary>
        ITransactionMetaData<ITag> Tags { get; }
    }
}
