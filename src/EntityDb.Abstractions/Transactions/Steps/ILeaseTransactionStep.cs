using EntityDb.Abstractions.Leases;

namespace EntityDb.Abstractions.Transactions.Steps
{
    /// <summary>
    ///     Represents a modification to an entity's leases.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be modified.</typeparam>
    public interface ILeaseTransactionStep<TEntity> : ITransactionStep<TEntity>
    {
        /// <summary>
        ///     The leases of the entity.
        /// </summary>
        ITransactionMetaData<ILease> Leases { get; }

        /// <summary>
        ///     The version number of the entity to record when leases are inserted.
        /// </summary>
        ulong LeasedAtEntityVersionNumber { get; }
    }
}
