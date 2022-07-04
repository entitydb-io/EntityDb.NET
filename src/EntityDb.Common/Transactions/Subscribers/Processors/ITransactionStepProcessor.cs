using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal interface ISnapshotTransactionStepProcessor<TSnapshot>
{
    Task<(TSnapshot?, TSnapshot)?> GetSnapshots(ITransaction transaction, ITransactionStep transactionStep, CancellationToken cancellationToken);
}
