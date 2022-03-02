using EntityDb.Abstractions.Leases;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a modification to an entity's leases.
/// </summary>
public interface ILeaseTransactionStep : ITransactionStep
{
    /// <summary>
    ///     The leases of the entity.
    /// </summary>
    ITransactionMetaData<ILease> Leases { get; }

    /// <summary>
    ///     The version number to record when leases are inserted.
    /// </summary>
    ulong LeasedAtEntityVersionNumber { get; }
}
