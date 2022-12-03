using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.Processors;

namespace EntityDb.Common.Transactions.Subscribers.ProcessorQueues;

internal class TestModeTransactionQueue<TTransactionProcessor> : ITransactionProcessorQueue<TTransactionProcessor>
    where TTransactionProcessor : ITransactionProcessor
{
    private readonly TTransactionProcessor _transactionProcessor;

    public TestModeTransactionQueue(TTransactionProcessor transactionProcessor)
    {
        _transactionProcessor = transactionProcessor;
    }

    public void Enqueue(ITransaction transaction)
    {
        Task.Run(() => _transactionProcessor.ProcessTransaction(transaction, default)).Wait();
    }
}
