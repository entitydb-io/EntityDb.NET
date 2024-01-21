using EntityDb.Abstractions.Sources;

namespace EntityDb.Common.Sources.Processors.Queues;

/// <summary>
///     Extensions for <see cref="ISourceProcessorQueue" />.
/// </summary>
public static class SourceProcessorQueueExtensions
{
    /// <summary>
    ///     Adds a source and the corresponding processor to the queue.
    /// </summary>
    /// <typeparam name="TSourceProcessor">The type of the <see cref="ISourceProcessor" /></typeparam>
    /// <param name="sourceProcessorQueue">The source processor queue</param>
    /// <param name="source">The source to process</param>
    public static void Enqueue<TSourceProcessor>(this ISourceProcessorQueue sourceProcessorQueue, Source source)
        where TSourceProcessor : ISourceProcessor
    {
        sourceProcessorQueue.Enqueue(new SourceProcessorQueueItem
        {
            SourceProcessorType = typeof(TSourceProcessor), Source = source,
        });
    }
}
