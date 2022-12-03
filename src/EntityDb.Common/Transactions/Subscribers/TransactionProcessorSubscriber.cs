using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.ProcessorQueues;
using EntityDb.Common.Transactions.Subscribers.Processors;

namespace EntityDb.Common.Transactions.Subscribers;

internal sealed class TransactionProcessorSubscriber<TTransactionProcessor> : ITransactionSubscriber
    where TTransactionProcessor : ITransactionProcessor
{
    private readonly ITransactionProcessorQueue<TTransactionProcessor> _transactionProcessorQueue;

    public TransactionProcessorSubscriber(ITransactionProcessorQueue<TTransactionProcessor> transactionProcessorQueue)
    {
        _transactionProcessorQueue = transactionProcessorQueue;
    }

    public void Notify(ITransaction transaction)
    {
        _transactionProcessorQueue.Enqueue(transaction);
    }
}
