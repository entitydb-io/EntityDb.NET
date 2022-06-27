using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions.Subscribers;

internal sealed class TransactionProcessorSubscriber<TTransactionProcessor> : TransactionSubscriber
    where TTransactionProcessor : ITransactionProcessor
{
    private readonly ILogger<TransactionProcessorSubscriber<TTransactionProcessor>> _logger;
    private readonly TTransactionProcessor _transactionProcessor;

    public TransactionProcessorSubscriber(ILogger<TransactionProcessorSubscriber<TTransactionProcessor>> logger,
        TTransactionProcessor transactionProcessor, bool testMode) : base(testMode)
    {
        _logger = logger;
        _transactionProcessor = transactionProcessor;
    }

    protected override async Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            await _transactionProcessor.ProcessTransaction(transaction, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred while processing transaction");
        }
    }

    public static TransactionProcessorSubscriber<TTransactionProcessor> Create(IServiceProvider serviceProvider,
        TTransactionProcessor transactionProcessor, bool testMode)
    {
        return ActivatorUtilities.CreateInstance<TransactionProcessorSubscriber<TTransactionProcessor>>(
            serviceProvider, transactionProcessor, testMode);
    }
}
