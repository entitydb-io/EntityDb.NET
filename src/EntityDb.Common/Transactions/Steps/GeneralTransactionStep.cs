using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using System;

namespace EntityDb.Common.Transactions.Steps
{
    internal sealed record GeneralTransactionStep<TEntity> : ICommandTransactionStep<TEntity>, ILeaseTransactionStep<TEntity>, ITagTransactionStep<TEntity>
    {
        public Guid EntityId { get; init; }
        public ICommand<TEntity> Command { get; init; } = default!;
        public TEntity PreviousEntitySnapshot { get; init; } = default!;
        public ulong PreviousEntityVersionNumber { get; init; }
        public TEntity NextEntitySnapshot { get; init; } = default!;
        public ulong NextEntityVersionNumber { get; init; }
        public ITransactionMetaData<ILease> Leases { get; init; } = default!;
        public ulong LeasedAtEntityVersionNumber => NextEntityVersionNumber;
        public ITransactionMetaData<ITag> Tags { get; init; } = default!;
        public ulong TaggedAtEntityVersionNumber => NextEntityVersionNumber;
    }
}
