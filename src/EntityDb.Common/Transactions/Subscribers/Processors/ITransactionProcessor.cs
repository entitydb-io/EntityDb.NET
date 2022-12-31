using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.Reprocessors;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

/// <summary>
///     Represents a type that processes transactions emitted to a <see cref="ITransactionSubscriber"/>.
/// </summary>
public interface ITransactionProcessor
{
    /// <summary>
    ///     Used for targeted reprocessing with <see cref="IReprocessTransactionsRequest"/>.
    /// </summary>
    string Identifier { get; }

    /// <summary>
    ///     Defines the procedure for processing a given transaction.
    /// </summary>
    /// <param name="transaction">The transaction that has been received.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task</returns>
    Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken);
}
