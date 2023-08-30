using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Sources.Processors.Queues;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Sources.ReprocessorQueues;

internal class TestModeTransactionReprocessorQueue : ITransactionReprocessorQueue
{
    private readonly ILogger<TestModeTransactionReprocessorQueue> _logger;
    private readonly ITransactionRepositoryFactory _transactionRepositoryFactory;
    private readonly ISourceProcessorQueue _transactionProcessorQueue;

    public TestModeTransactionReprocessorQueue(ILogger<TestModeTransactionReprocessorQueue> logger, ITransactionRepositoryFactory transactionRepositoryFactory, ISourceProcessorQueue transactionProcessorQueue)
    {
        _logger = logger;
        _transactionRepositoryFactory = transactionRepositoryFactory;
        _transactionProcessorQueue = transactionProcessorQueue;
    }

    public void Enqueue(ITransactionReprocessorQueueItem reprocessTransactionsRequest)
    {
        Task.Run(() => Process(reprocessTransactionsRequest, default));
    }

    protected async Task Process(ITransactionReprocessorQueueItem item, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Started reprocessing transactions");

            await using var transactionRepository = await _transactionRepositoryFactory.CreateRepository(item.TransactionSessionOptionsName, cancellationToken);

            var transactionIds = await transactionRepository
                .EnumerateTransactionIds(item.Query, cancellationToken)
                .ToArrayAsync(cancellationToken);

            foreach (var transactionId in transactionIds)
            {
                var transaction = await transactionRepository
                    .GetTransaction(transactionId, cancellationToken);

                _transactionProcessorQueue.Enqueue(new SourceProcessorQueueItem
                {
                    SourceProcessorType = item.TransactionProcessorType,
                    Source = transaction,
                });
            }

            _logger.LogDebug("Finished reprocessing transactions");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to reprocess transactions");
        }
    }
}
