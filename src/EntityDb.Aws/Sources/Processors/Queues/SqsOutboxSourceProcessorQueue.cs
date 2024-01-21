using Amazon.SQS.Model;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Sources.Processors.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Aws.Sources.Processors.Queues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal sealed class SqsOutboxSourceProcessorQueue : BackgroundService, ISourceProcessorQueue
{
    private readonly BufferBlock<ISourceProcessorQueueItem> _bufferBlock = new();
    private readonly ILogger<SqsOutboxSourceProcessorQueue> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SqsSourceProcessorQueueOptions _options;
    private CancellationTokenSource? _linkedStoppingTokenSource;

    public SqsOutboxSourceProcessorQueue
    (
        ILogger<SqsOutboxSourceProcessorQueue> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsFactory<SqsSourceProcessorQueueOptions> optionsFactory,
        string optionsName
    )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _options = optionsFactory.Create(optionsName);
    }

    public void Enqueue(ISourceProcessorQueueItem item)
    {
        if (_linkedStoppingTokenSource is { IsCancellationRequested: true })
        {
            _logger.LogWarning("Application is shutting down when messages are still being received");
        }
        
        _bufferBlock.Post(item);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var linkedStoppingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        _linkedStoppingTokenSource = linkedStoppingTokenSource;
        
        CancellationToken cancellationToken = default;
        
        while (await _bufferBlock.OutputAvailableAsync(cancellationToken))
        {
            if (_linkedStoppingTokenSource.IsCancellationRequested)
            {   
                _logger.LogWarning("Application is shutting down when messages are still being enqueued");
            }
            
            await Task.Delay(_options.EnqueueDelay, cancellationToken);
            
            var item = await _bufferBlock.ReceiveAsync(cancellationToken);

            await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
            
            var (amazonSqs, queueUrl) = await _options.AmazonSqsFactory.Invoke(serviceScope.ServiceProvider);

            using var logScope = _logger.BeginScope(new KeyValuePair<string, object>[]
            {
                new("QueueUrl", queueUrl),
                new("SourceProcessorType", item.SourceProcessorType.Name),
                new("SourceId", item.Source.Id.Value),
            });

            try
            {
                _logger.Log(_options.DebugLogLevel, "Started enqueueing source");
                
                var sourceProcessorEnvelopeHeaders = EnvelopeHelper.GetEnvelopeHeaders(item.SourceProcessorType);
                var sourceProcessorTypeName = sourceProcessorEnvelopeHeaders.Value[EnvelopeHelper.Type];

                var sourceId = item.Source.Id.Value;
                
                foreach (var message in item.Source.Messages.DistinctBy(message => message.StatePointer.Id))
                {
                    var stateId = message.StatePointer.Id.Value;

                    var sendMessageResponse = await amazonSqs.SendMessageAsync
                    (
                        new SendMessageRequest
                        {
                            QueueUrl = queueUrl,
                            MessageBody = JsonSerializer.Serialize(new SqsSourceProcessorMessage
                            {
                                StateId = stateId,
                                SourceId = sourceId,
                                SourceRepositoryOptionsName =
                                    _options.GetSourceRepositoryOptionsNameFromDelta(message.Delta),
                                SourceProcessorEnvelopeHeaders = sourceProcessorEnvelopeHeaders,
                            }),
                            MessageGroupId = $"{sourceProcessorTypeName}:{stateId}",
                            MessageDeduplicationId = $"{sourceId}",
                        },
                        cancellationToken
                    );

                    _logger.Log(_options.DebugLogLevel,
                        "Message {MessageId} enqueued for {SourceId} and {StateId} using {SourceProcessorTypeName}",
                        sendMessageResponse.MessageId, sourceId, stateId, sourceProcessorTypeName);
                }

                _logger.Log(_options.DebugLogLevel, "Finished enqueueing source");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while enqueueing source");
            }
        }

        while (await _bufferBlock.OutputAvailableAsync(cancellationToken))
        {
        }
    }
}
