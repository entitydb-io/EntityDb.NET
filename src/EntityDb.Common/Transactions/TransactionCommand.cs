using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using System;

namespace EntityDb.Common.Transactions
{
    internal sealed record TransactionCommand<TEntity> : ITransactionCommand<TEntity>
    {
        public TEntity EntitySnapshot { get; init; } = default!;
        public Guid EntityId { get; init; }
        public ulong EntityVersionNumber { get; init; }
        public ICommand<TEntity> Command { get; init; } = default!;
        public ITransactionMetaData<ILease> Leases { get; init; } = default!;
        public ITransactionMetaData<ITag> Tags { get; init; } = default!;
    }
}
