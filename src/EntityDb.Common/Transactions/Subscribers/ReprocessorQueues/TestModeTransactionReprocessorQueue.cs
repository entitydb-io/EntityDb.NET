using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.Processors;
using EntityDb.Common.Transactions.Subscribers.Reprocessors;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Transactions.Subscribers.ReprocessorQueues;

internal class TestModeTransactionReprocessorQueue : TransactionReprocessorQueueBase, ITransactionReprocessorQueue
{
    public TestModeTransactionReprocessorQueue(ILogger<TestModeTransactionReprocessorQueue> logger, ITransactionRepositoryFactory transactionRepositoryFactory, IEnumerable<ITransactionProcessor> transactionProcessors) : base(logger, transactionRepositoryFactory, transactionProcessors)
    {
    }

    public void Enqueue(IReprocessTransactionsRequest reprocessTransactionsRequest)
    {
        Task.Run(() => Process(reprocessTransactionsRequest, default));
    }
}
