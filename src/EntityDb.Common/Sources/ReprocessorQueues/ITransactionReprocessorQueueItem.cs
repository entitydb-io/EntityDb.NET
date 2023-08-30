using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Sources.Processors;

namespace EntityDb.Common.Sources.ReprocessorQueues;

/// <summary>
///     Represents a request for a <see cref="ISourceProcessor"/> to reprocess transactions.
/// </summary>
public interface ITransactionReprocessorQueueItem
{
    /// <summary>
    ///     The name of the transaction session options passed to <see cref="ITransactionRepositoryFactory.CreateRepository(string, CancellationToken)"/>
    /// </summary>
    string TransactionSessionOptionsName { get; }

    /// <summary>
    ///     The type of the transaction process, which *must*
    ///     implement <see cref="ISourceProcessor"/>.
    /// </summary>
    Type TransactionProcessorType { get; }

    /// <summary>
    ///     Determines which transactions need to be reprocessed.
    /// </summary>
    IQuery Query { get; }

    /// <summary>
    ///     Determines how long to wait between each call to enqueue.
    /// </summary>
    TimeSpan EnqueueDelay { get; }

    /// <summary>
    ///     If <c>true</c>, then stop executing this request when the transaction processor throws an exception.
    ///     Otherwise, execute this request for all matching transactions.
    /// </summary>
    bool BreakOnThrow { get; }
}
