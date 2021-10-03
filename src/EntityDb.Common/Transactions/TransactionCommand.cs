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
        public ImmutableArray<ILease> DeleteLeases { get; init; }
        public ImmutableArray<ILease> InsertLeases { get; init; }
        public ImmutableArray<ITag> DeleteTags { get; init; }
        public ImmutableArray<ITag> InsertTags { get; init; }
    }
}
