namespace EntityDb.Common.Sources.Processors.Queues;

/// <summary>
///     A service for queueing source processing work
/// </summary>
public interface ISourceProcessorQueue
{
    /// <summary>
    ///     Adds an item to the queue
    /// </summary>
    /// <param name="item">The work to be queued</param>
    void Enqueue(ISourceProcessorQueueItem item);
}
