using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Common.Sources.Processors.Queues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal class BufferBlockSourceProcessorQueue : BackgroundService, ISourceProcessorQueue
{
    private readonly BufferBlock<ISourceProcessorQueueItem> _bufferBlock = new();
    private readonly ILogger<BufferBlockSourceProcessorQueue> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BufferBlockSourceProcessorQueue(ILogger<BufferBlockSourceProcessorQueue> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void Enqueue(ISourceProcessorQueueItem item)
    {
        _bufferBlock.Post(item);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _bufferBlock.OutputAvailableAsync(stoppingToken))
        {
            var item = await _bufferBlock.ReceiveAsync(stoppingToken);

            await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();

            using var logScope = _logger.BeginScope(new KeyValuePair<string, object>[]
            {
                new("SourceProcessorType", item.SourceProcessorType.Name),
                new("SourceId", item.Source.Id.Value)
            });

            try
            {
                var sourceProcessor = (ISourceProcessor)serviceScope.ServiceProvider.GetRequiredService(item.SourceProcessorType);

                _logger.LogDebug("Started processing source");

                await sourceProcessor.Process(item.Source, stoppingToken);

                _logger.LogDebug("Finished processing source");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while processing source");
            }
        }
    }
}
