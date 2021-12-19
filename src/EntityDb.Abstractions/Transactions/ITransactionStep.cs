using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using System;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    ///     Represents a set of modifiers for a single entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be modified.</typeparam>
    public interface ITransactionStep<TEntity>
    {
        /// <summary>
        ///     A snapshot of the entity before the command.
        /// </summary>
        TEntity PreviousEntitySnapshot { get; }

        /// <summary>
        ///     The previous version number of the entity.
        /// </summary>
        ulong PreviousEntityVersionNumber { get; }

        /// <summary>
        ///     A snapshot of the entity after the command.
        /// </summary>
        TEntity NextEntitySnapshot { get; }

        /// <summary>
        ///     The next version number of the entity.
        /// </summary>
        ulong NextEntityVersionNumber { get; }

        /// <summary>
        ///     The id of the entity.
        /// </summary>
        Guid EntityId { get; }

        /// <summary>
        ///     The intent.
        /// </summary>
        ICommand<TEntity> Command { get; }

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
