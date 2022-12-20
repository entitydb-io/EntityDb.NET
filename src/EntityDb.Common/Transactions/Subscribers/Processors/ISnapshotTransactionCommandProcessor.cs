using EntityDb.Abstractions.Transactions;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal interface ISnapshotTransactionCommandProcessor<TSnapshot>
{
    Task<(TSnapshot?, TSnapshot)?> GetSnapshots(ITransaction transaction, ITransactionCommand transactionCommand, CancellationToken cancellationToken);
}
