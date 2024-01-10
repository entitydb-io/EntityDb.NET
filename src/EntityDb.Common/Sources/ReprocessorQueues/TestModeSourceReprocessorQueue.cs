using EntityDb.Abstractions.Sources;
using EntityDb.Common.Sources.Processors.Queues;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Sources.ReprocessorQueues;

internal class TestModeSourceReprocessorQueue : ISourceReprocessorQueue
{
    private readonly ILogger<TestModeSourceReprocessorQueue> _logger;
    private readonly ISourceProcessorQueue _sourceProcessorQueue;
    private readonly ISourceRepositoryFactory _sourceRepositoryFactory;

    public TestModeSourceReprocessorQueue(ILogger<TestModeSourceReprocessorQueue> logger,
        ISourceRepositoryFactory sourceRepositoryFactory, ISourceProcessorQueue sourceProcessorQueue)
    {
        _logger = logger;
        _sourceRepositoryFactory = sourceRepositoryFactory;
        _sourceProcessorQueue = sourceProcessorQueue;
    }

    public void Enqueue(ISourceReprocessorQueueItem reprocessSourcesRequest)
    {
        Task.Run(() => Process(reprocessSourcesRequest, default));
    }

    private async Task Process(ISourceReprocessorQueueItem item, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Started reprocessing sources");

            await using var sourceRepository =
                await _sourceRepositoryFactory.Create(item.SourceSessionOptionsName,
                    cancellationToken);

            var sourceIds = await sourceRepository
                .EnumerateSourceIds(item.Query, cancellationToken)
                .ToArrayAsync(cancellationToken);

            foreach (var sourceId in sourceIds)
            {
                var source = await sourceRepository
                    .GetSource(sourceId, cancellationToken);

                _sourceProcessorQueue.Enqueue(new SourceProcessorQueueItem
                {
                    SourceProcessorType = item.SourceProcessorType, Source = source,
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
