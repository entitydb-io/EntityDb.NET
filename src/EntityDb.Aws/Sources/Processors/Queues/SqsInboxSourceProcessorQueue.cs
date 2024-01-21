using Amazon.SQS.Model;
using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Sources;
using EntityDb.Common.Sources.Processors;
using EntityDb.Common.TypeResolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace EntityDb.Aws.Sources.Processors.Queues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal sealed class SqsInboxSourceProcessorQueue : BackgroundService
{
    private readonly ILogger<SqsInboxSourceProcessorQueue> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ITypeResolver _typeResolver;
    private readonly SqsSourceProcessorQueueOptions _options;

    public SqsInboxSourceProcessorQueue
    (
        ILogger<SqsInboxSourceProcessorQueue> logger,
        IServiceScopeFactory serviceScopeFactory,
        ITypeResolver typeResolver,
        IOptionsFactory<SqsSourceProcessorQueueOptions> optionsFactory,
        string sqsOptionsName
    )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _typeResolver = typeResolver;
        _options = optionsFactory.Create(sqsOptionsName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int maxNumberOfMessages = 1;
        
        var active = true;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(active ? _options.ActiveDequeueDelay : _options.IdleDequeueDelay, stoppingToken);
            
            await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
            
            var (amazonSqs, queueUrl) = await _options.AmazonSqsFactory.Invoke(serviceScope.ServiceProvider);
            
            var receiveMessageResponse = await amazonSqs.ReceiveMessageAsync
            (
                new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = maxNumberOfMessages,
                },
                stoppingToken
            );
            
            if (receiveMessageResponse.Messages.Count != maxNumberOfMessages)
            {
                active = false;
                continue;
            }
            
            active = true;

            var receivedMessage = receiveMessageResponse.Messages[0];
            
            var deleteRequest = new DeleteMessageRequest
            {
                ReceiptHandle = receivedMessage.ReceiptHandle,
            };
            
            var sqsSourceProcessorMessage =
                JsonSerializer.Deserialize<SqsSourceProcessorMessage>(receivedMessage.Body) ??
                throw new UnreachableException();
            
            var sourceId = new Id(sqsSourceProcessorMessage.SourceId);
            var stateId = new Id(sqsSourceProcessorMessage.StateId);
            
            await using var sourceRepositoryFactory =
                serviceScope.ServiceProvider.GetRequiredService<ISourceRepositoryFactory>();

            await using var sourceRepository = await sourceRepositoryFactory.Create(sqsSourceProcessorMessage.SourceRepositoryOptionsName,
                stoppingToken);
            
            var sourceProcessorTypeName = sqsSourceProcessorMessage.SourceProcessorEnvelopeHeaders.Value[EnvelopeHelper.Type];
            
            using var logScope = _logger.BeginScope(new KeyValuePair<string, object>[]
            {
                new("QueueUrl", queueUrl),
                new("MessageId", receivedMessage.MessageId),
                new("ReceiptHandle", receivedMessage.ReceiptHandle),
                new("SourceProcessorType", sourceProcessorTypeName),
                new("SourceId", sqsSourceProcessorMessage.SourceId),
                new("StateId", sqsSourceProcessorMessage.StateId),
            });
            
            try
            {
                var source = await sourceRepository.GetSource(sourceId, stateId, stoppingToken);

                var sourceProcessorType = _typeResolver.ResolveType(sqsSourceProcessorMessage.SourceProcessorEnvelopeHeaders);
                
                var sourceProcessor =
                    (ISourceProcessor)serviceScope.ServiceProvider.GetRequiredService(sourceProcessorType);

                _logger.Log(_options.DebugLogLevel, "Started processing source");

                await sourceProcessor.Process(source, stoppingToken);
                
                _logger.Log(_options.DebugLogLevel, "Finished processing source");
                
                await amazonSqs.DeleteMessageAsync(deleteRequest, stoppingToken);

                _logger.Log(LogLevel.Debug, "Message deleted from queue");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while processing source");
            }
        }
    }
}
