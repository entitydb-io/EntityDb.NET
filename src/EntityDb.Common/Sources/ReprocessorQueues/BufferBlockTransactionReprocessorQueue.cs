using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Sources.Processors.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Common.Sources.ReprocessorQueues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal class BufferBlockTransactionReprocessorQueue : BackgroundService, ITransactionReprocessorQueue
{
    private readonly BufferBlock<ITransactionReprocessorQueueItem> _bufferBlock = new();
    private readonly ILogger<BufferBlockTransactionReprocessorQueue> _logger;
    private readonly ITransactionRepositoryFactory _transactionRepositoryFactory;
    private readonly ISourceProcessorQueue _sourceProcessorQueue;

    public BufferBlockTransactionReprocessorQueue(ILogger<BufferBlockTransactionReprocessorQueue> logger, ITransactionRepositoryFactory transactionRepositoryFactory, ISourceProcessorQueue sourceProcessorQueue)
    {
        _logger = logger;
        _transactionRepositoryFactory = transactionRepositoryFactory;
        _sourceProcessorQueue = sourceProcessorQueue;
    }

    public void Enqueue(ITransactionReprocessorQueueItem reprocessTransactionsRequest)
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

    protected async Task Process(ITransactionReprocessorQueueItem item, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Started reprocessing sources");

            await using var transactionRepository = await _transactionRepositoryFactory.CreateRepository(item.TransactionSessionOptionsName, cancellationToken);

            var transactionIds = await transactionRepository
                .EnumerateTransactionIds(item.Query, cancellationToken)
                .ToArrayAsync(cancellationToken);

            foreach (var transactionId in transactionIds)
            {
                await Task.Delay(item.EnqueueDelay, cancellationToken);

                var transaction = await transactionRepository
                    .GetTransaction(transactionId, cancellationToken);

                _sourceProcessorQueue.Enqueue(new SourceProcessorQueueItem
                {
                    SourceProcessorType = item.SourceProcessorType,
                    Source = transaction,
                });
            }

            _logger.LogDebug("Finished reprocessing sources");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to reprocess sources");
        }
    }
}
