using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using System;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions
{
    internal sealed record TransactionCommand<TEntity> : ITransactionCommand<TEntity>
    {
        public TEntity? PreviousSnapshot { get; init; }
        public TEntity NextSnapshot { get; init; } = default!;
        public Guid EntityId { get; init; }
        public ulong ExpectedPreviousVersionNumber { get; init; }
        public ICommand<TEntity> Command { get; init; } = default!;
        public ImmutableArray<ITransactionFact<TEntity>> Facts { get; init; }
        public ITransactionMetaData<ILease> Leases { get; init; } = default!;
        public ITransactionMetaData<ITag> Tags { get; init; } = default!;
    }
}
