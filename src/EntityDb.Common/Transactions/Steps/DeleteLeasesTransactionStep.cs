using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Transactions.Steps;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions.Steps;

internal sealed record DeleteLeasesTransactionStep : TransactionStepBase, IDeleteLeasesTransactionStep
{
    public ImmutableArray<ILease> Leases { get; init; }
}
