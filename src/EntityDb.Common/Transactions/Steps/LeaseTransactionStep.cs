using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using System;

namespace EntityDb.Common.Transactions.Steps
{
    internal sealed record LeaseTransactionStep<TEntity> : ILeaseTransactionStep<TEntity>
    {
        public Guid EntityId { get; init; }
        public ulong LeasedAtEntityVersionNumber { get; init; }
        public ITransactionMetaData<ILease> Leases { get; init; } = default!;
    }
}
