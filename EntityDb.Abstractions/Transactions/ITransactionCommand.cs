using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using System;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    /// Represents a set of modifiers for a single entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be modified.</typeparam>
    public interface ITransactionCommand<TEntity>
    {
        /// <summary>
        /// A snapshot of the entity before the command.
        /// </summary>
        TEntity? PreviousSnapshot { get; }

        /// <summary>
        /// A snapshot of the entity after the command.
        /// </summary>
        TEntity NextSnapshot { get; }

        /// <summary>
        /// The id of the entity.
        /// </summary>
        Guid EntityId { get; }

        /// <summary>
        /// The previous version number of the entity.
        /// </summary>
        /// <remarks>
        /// The repository must use a VersionNumber equal to <see cref="ExpectedPreviousVersionNumber"/> + 1.
        /// </remarks>
        ulong ExpectedPreviousVersionNumber { get; }

        /// <summary>
        /// The intent.
        /// </summary>
        ICommand<TEntity> Command { get; }

        /// <summary>
        /// The set of modifiers.
        /// </summary>
        /// <remarks>
        /// <see cref="Facts"/> does not need to be ordered, but each entry must have a unique <see cref="ITransactionFact{TEntity}.SubversionNumber"/> for the given <see cref="ExpectedPreviousVersionNumber"/>.
        /// 
        /// Allthough it may seem awkward, this provides two benefits.
        /// 1. None of the repository implementations need to implement subversion number counting.
        /// 2. The generic test suite can verify that the uniqueness constraint is satisfied.
        /// </remarks>
        ITransactionFact<TEntity>[] Facts { get; }

        /// <summary>
        /// The metadata properties which must be removed.
        /// </summary>
        /// <remarks>
        /// <see cref="DeleteLeases"/> must be deleted from the repository before <see cref="InsertLeases"/> are inserted into the repository.
        /// </remarks>
        ILease[] DeleteLeases { get; }

        /// <summary>
        /// The metadata properties which must be added.
        /// </summary>
        /// <remarks>
        /// <see cref="DeleteLeases"/> must be deleted from the repository before <see cref="InsertLeases"/> are inserted into the repository.
        /// </remarks>
        ILease[] InsertLeases { get; }
    }
}
