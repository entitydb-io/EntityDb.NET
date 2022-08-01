using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal interface ISnapshotTransactionStepProcessor<TSnapshot>
{
    Task<(TSnapshot?, TSnapshot)?> GetSnapshots(ITransaction transaction, ITransactionStep transactionStep, CancellationToken cancellationToken);
}
