using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Transactions;
using System;

namespace EntityDb.Common.Transactions
{
    internal sealed record TransactionCommand<TEntity>(Guid EntityId, ulong ExpectedPreviousVersionNumber, ICommand<TEntity> Command, ITransactionFact<TEntity>[] Facts, ILease[] DeleteLeases, ILease[] InsertLeases) : ITransactionCommand<TEntity>
    {
    }
}
