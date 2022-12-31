using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.Processors;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Transactions.Subscribers.Reprocessors;

/// <summary>
///     Represents a request for a <see cref="ITransactionProcessor"/> to reprocess transactions.
/// </summary>
public interface IReprocessTransactionsRequest
{
    /// <summary>
    ///     The name of the transaction session options passed to <see cref="ITransactionRepositoryFactory.CreateRepository(string, CancellationToken)"/>
    /// </summary>
    string TransactionSessionOptionsName { get; }

    /// <summary>
    ///     Determines which transaction processor needs to reprocess the transactions, based on the value of <see cref="ITransactionProcessor.Identifier"/>.
    /// </summary>
    string TransactionProcessorIdentifier { get; }

    /// <summary>
    ///     Determines which transactions need to be reprocessed.
    /// </summary>
    ICommandQuery CommandQuery { get; }

    /// <summary>
    ///     If <c>true</c>, then stop executing this request when the transaction processor throws an exception.
    ///     Otherwise, execute this request for all matching transactions.
    /// </summary>
    bool BreakOnThrow { get; }

    /// <summary>
    ///     Converts this request into a log-friendly format.
    /// </summary>
    /// <returns>The object that will be passed to <see cref="ILogger.BeginScope{TState}(TState)"/></returns>
    object ToLogScopeState();
}
