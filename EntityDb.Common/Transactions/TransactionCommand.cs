using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using System;

namespace EntityDb.Common.Transactions
{
    internal sealed record TransactionCommand<TEntity>(Guid EntityId, ulong ExpectedPreviousVersionNumber, ICommand<TEntity> Command, ITransactionFact<TEntity>[] Facts, ITag[] DeleteTags, ITag[] InsertTags) : ITransactionCommand<TEntity>
    {
    }
}
