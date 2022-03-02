using EntityDb.Abstractions.Leases;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a transaction step that adds leases.
/// </summary>
public interface IAddLeasesTransactionStep : ITransactionStep
{
    /// <summary>
    ///     The leases that need to be added.
    /// </summary>
    ImmutableArray<ILease> Leases { get; }
}
