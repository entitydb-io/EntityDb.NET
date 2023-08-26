using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.Processors;
using EntityDb.Common.Transactions.Subscribers.Reprocessors;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Common.Transactions.Subscribers.ReprocessorQueues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal class BufferBlockTransactionReprocessorQueue : TransactionReprocessorQueueBase, ITransactionReprocessorQueue
{
    private readonly BufferBlock<IReprocessTransactionsRequest> _bufferBlock = new();

    public BufferBlockTransactionReprocessorQueue(ILogger<BufferBlockTransactionReprocessorQueue> logger, ITransactionRepositoryFactory transactionRepositoryFactory, IEnumerable<ITransactionProcessor> transactionProcessors) : base(logger, transactionRepositoryFactory, transactionProcessors)
    {
    }

    public void Enqueue(IReprocessTransactionsRequest reprocessTransactionsRequest)
    {
        _bufferBlock.Post(reprocessTransactionsRequest);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _bufferBlock.OutputAvailableAsync(stoppingToken))
        {
            var reprocessTransactionsRequest = await _bufferBlock.ReceiveAsync(stoppingToken);

            await Process(reprocessTransactionsRequest, stoppingToken);
        }
    }
}
