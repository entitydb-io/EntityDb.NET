using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Transactions.Subscribers.Processors;
using EntityDb.Common.Transactions.Subscribers.Reprocessors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Transactions.Subscribers.ReprocessorQueues;

internal abstract class TransactionReprocessorQueueBase : BackgroundService
{
    private readonly ILogger _logger;
    private readonly ITransactionRepositoryFactory _transactionRepositoryFactory;
    private readonly IEnumerable<ITransactionProcessor> _transactionProcessors;

    protected TransactionReprocessorQueueBase(ILogger logger, ITransactionRepositoryFactory transactionRepositoryFactory, IEnumerable<ITransactionProcessor> transactionProcessors)
    {
        _logger = logger;
        _transactionRepositoryFactory = transactionRepositoryFactory;
        _transactionProcessors = transactionProcessors;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // I don't like that I have to do this, but it's the only decent way
        // to share the Process method between BufferBlock and TestMode implementations
        throw new NotSupportedException();
    }

    protected async Task Process(IReprocessTransactionsRequest reprocessTransactionsRequest, CancellationToken cancellationToken)
    {
        using var outerScope = _logger.BeginScope(reprocessTransactionsRequest.ToLogScopeState());

        try
        {
            _logger.LogDebug("Started reprocessing transactions");

            var transactionProcessor = _transactionProcessors
                .Single(transactionProcessor => transactionProcessor.Identifier == reprocessTransactionsRequest.TransactionProcessorIdentifier);

            await using var transactionRepository = await _transactionRepositoryFactory.CreateRepository(reprocessTransactionsRequest.TransactionSessionOptionsName, cancellationToken);

            var transactions = transactionRepository.EnumerateTransactions(reprocessTransactionsRequest.CommandQuery, cancellationToken);

            await foreach (var transaction in transactions)
            {
                using var innerScope = _logger.BeginScope(new KeyValuePair<string, object>[]
                {
                    new("TransactionId", transaction.Id.Value),
                });

                try
                {
                    await transactionProcessor.ProcessTransaction(transaction, cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to reprocess transaction");

                    if (reprocessTransactionsRequest.BreakOnThrow)
                    {
                        break;
                    }
                }
            }

            _logger.LogDebug("Finished reprocessing transactions");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to reprocess transactions");
        }
    }
}
