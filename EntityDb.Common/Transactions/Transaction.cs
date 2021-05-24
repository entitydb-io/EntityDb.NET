using EntityDb.Abstractions.Transactions;
using System;

namespace EntityDb.Common.Transactions
{
    internal sealed record Transaction<TEntity>(Guid Id, DateTime TimeStamp, object Source, ITransactionCommand<TEntity>[] Commands) : ITransaction<TEntity>
    {
    }
}
