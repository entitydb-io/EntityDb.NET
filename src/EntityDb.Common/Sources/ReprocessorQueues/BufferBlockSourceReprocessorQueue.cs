using EntityDb.Abstractions.Sources;
using EntityDb.Common.Sources.Processors.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Common.Sources.ReprocessorQueues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal sealed class BufferBlockSourceReprocessorQueue : BackgroundService, ISourceReprocessorQueue
{
    private readonly BufferBlock<ISourceReprocessorQueueItem> _bufferBlock = new();
    private readonly ILogger<BufferBlockSourceReprocessorQueue> _logger;
    private readonly ISourceProcessorQueue _sourceProcessorQueue;
    private readonly ISourceRepositoryFactory _sourceRepositoryFactory;

    public BufferBlockSourceReprocessorQueue(ILogger<BufferBlockSourceReprocessorQueue> logger,
        ISourceRepositoryFactory sourceRepositoryFactory, ISourceProcessorQueue sourceProcessorQueue)
    {
        _logger = logger;
        _sourceRepositoryFactory = sourceRepositoryFactory;
        _sourceProcessorQueue = sourceProcessorQueue;
    }

    public void Enqueue(ISourceReprocessorQueueItem reprocessSourcesRequest)
    {
        _bufferBlock.Post(reprocessSourcesRequest);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _bufferBlock.OutputAvailableAsync(stoppingToken))
        {
            var sourceReprocessorQueueItem = await _bufferBlock.ReceiveAsync(stoppingToken);

            await Process(sourceReprocessorQueueItem, stoppingToken);
        }
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
                .EnumerateSourceIds(item.DataQuery, cancellationToken)
                .ToArrayAsync(cancellationToken);

            foreach (var sourceId in sourceIds)
            {
                await Task.Delay(item.EnqueueDelay, cancellationToken);

                var source = await sourceRepository
                    .GetSource(sourceId, default, cancellationToken);

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
