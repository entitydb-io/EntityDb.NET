namespace EntityDb.Common.Sources.ReprocessorQueues;

/// <summary>
///     A queue that reprocesses sources
/// </summary>
public interface ISourceReprocessorQueue
{
    /// <summary>
    ///     Enqueues the request to reprocess sources
    /// </summary>
    /// <param name="item"></param>
    void Enqueue(ISourceReprocessorQueueItem item);
}
