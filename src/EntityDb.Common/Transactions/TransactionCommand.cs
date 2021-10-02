using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions
{
    internal sealed record TransactionCommand<TEntity>
    (
        TEntity? PreviousSnapshot,
        TEntity NextSnapshot,
        Guid EntityId,
        ulong ExpectedPreviousVersionNumber,
        ICommand<TEntity> Command,
        ImmutableArray<ITransactionFact<TEntity>> Facts,
        ImmutableArray<ILease> DeleteLeases,
        ImmutableArray<ILease> InsertLeases,
        ImmutableArray<ITag> DeleteTags,
        ImmutableArray<ITag> InsertTags
    ) : ITransactionCommand<TEntity>
    {
    }
}
