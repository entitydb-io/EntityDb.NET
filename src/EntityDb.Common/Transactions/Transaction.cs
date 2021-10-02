using EntityDb.Abstractions.Transactions;
using System;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions
{
    internal sealed record Transaction<TEntity>(Guid Id, DateTime TimeStamp, object Source, ImmutableArray<ITransactionCommand<TEntity>> Commands) : ITransaction<TEntity>
    {
    }
}
