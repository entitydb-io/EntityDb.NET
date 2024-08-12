using EntityDb.Aws.Sources.Processors.Queues;
using EntityDb.Common.Sources.Processors.Queues;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Aws.Extensions;

/// <summary>
///     Extensions for service collections.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a queue for processing sources as they are committed.
    ///     For test mode, the queue is not actually a queue and will immediately process the source.
    ///     For non-test mode, the implementation of ISourceProcessorQueue uses a buffer
    ///     block to receive messages, enqueue them to sqs, and then background-only service
    ///     will dequeue them from sqs and process them as normal.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="testMode">Whether or not to run in test mode.</param>
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static void AddSqsSourceProcessorQueue(this IServiceCollection serviceCollection,
        bool testMode)
    {
        if (testMode)
        {
            serviceCollection.AddSingleton<ISourceProcessorQueue, TestModeSourceProcessorQueue>();
        }
        else
        {
            serviceCollection.AddSingleton<SqsOutboxSourceProcessorQueue>();

            serviceCollection.AddSingleton<ISourceProcessorQueue>(serviceProvider =>
                serviceProvider.GetRequiredService<SqsOutboxSourceProcessorQueue>());

            serviceCollection.AddHostedService(serviceProvider =>
                serviceProvider.GetRequiredService<SqsOutboxSourceProcessorQueue>());
            
            serviceCollection.AddHostedService<SqsInboxSourceProcessorQueue>();
        }
    }
}
