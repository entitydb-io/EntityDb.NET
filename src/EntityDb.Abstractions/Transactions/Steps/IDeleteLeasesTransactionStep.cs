using EntityDb.Abstractions.Leases;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a transaction step that deletes leases.
/// </summary>
public interface IDeleteLeasesTransactionStep : ITransactionStep
{
    /// <summary>
    ///     The leases that need to be deleted.
    /// </summary>
    ImmutableArray<ILease> Leases { get; }
}
