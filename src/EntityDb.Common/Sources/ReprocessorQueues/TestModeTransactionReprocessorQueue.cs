using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Sources.Processors.Queues;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Sources.ReprocessorQueues;

internal class TestModeTransactionReprocessorQueue : ITransactionReprocessorQueue
{
    private readonly ILogger<TestModeTransactionReprocessorQueue> _logger;
    private readonly ITransactionRepositoryFactory _transactionRepositoryFactory;
    private readonly ISourceProcessorQueue _sourceProcessorQueue;

    public TestModeTransactionReprocessorQueue(ILogger<TestModeTransactionReprocessorQueue> logger, ITransactionRepositoryFactory transactionRepositoryFactory, ISourceProcessorQueue sourceProcessorQueue)
    {
        _logger = logger;
        _transactionRepositoryFactory = transactionRepositoryFactory;
        _sourceProcessorQueue = sourceProcessorQueue;
    }

    public void Enqueue(ITransactionReprocessorQueueItem reprocessTransactionsRequest)
    {
        Task.Run(() => Process(reprocessTransactionsRequest, default));
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
