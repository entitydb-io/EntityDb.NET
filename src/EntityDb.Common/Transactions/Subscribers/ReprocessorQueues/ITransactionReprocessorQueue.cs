using EntityDb.Common.Transactions.Subscribers.Reprocessors;

namespace EntityDb.Common.Transactions.Subscribers.ReprocessorQueues;

/// <summary>
///     A queue that reprocesses transactions
/// </summary>
public interface ITransactionReprocessorQueue
{
    /// <summary>
    ///     Enqueues the request to reprocess transactions
    /// </summary>
    /// <param name="reprocessTransactionsRequest"></param>
    void Enqueue(IReprocessTransactionsRequest reprocessTransactionsRequest);
}
