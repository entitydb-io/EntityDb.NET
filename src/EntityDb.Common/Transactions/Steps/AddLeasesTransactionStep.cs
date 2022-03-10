using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Transactions.Steps;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions.Steps;

internal sealed record AddLeasesTransactionStep : TransactionStepBase, IAddLeasesTransactionStep
{
    public ImmutableArray<ILease> Leases { get; init; }
}
