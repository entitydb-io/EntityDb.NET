namespace EntityDb.Common.Sources.ReprocessorQueues;

/// <summary>
///     A queue that reprocesses transactions
/// </summary>
public interface ITransactionReprocessorQueue
{
    /// <summary>
    ///     Enqueues the request to reprocess transactions
    /// </summary>
    /// <param name="item"></param>
    void Enqueue(ITransactionReprocessorQueueItem item);
}
