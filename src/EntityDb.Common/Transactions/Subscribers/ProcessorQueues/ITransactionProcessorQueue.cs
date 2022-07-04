using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.Processors;

namespace EntityDb.Common.Transactions.Subscribers.ProcessorQueues;

internal interface ITransactionProcessorQueue<TTransactionProcessor>
    where TTransactionProcessor : ITransactionProcessor
{
    void Enqueue(ITransaction transaction);
}
