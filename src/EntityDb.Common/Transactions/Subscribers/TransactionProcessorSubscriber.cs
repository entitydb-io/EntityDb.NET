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
    private readonly TTransactionProcessor _transactionProcessor;

    public TransactionProcessorSubscriber(ILogger<TransactionProcessorSubscriber<TTransactionProcessor>> logger,
        TTransactionProcessor transactionProcessor, bool testMode) : base(logger, testMode)
    {
        _transactionProcessor = transactionProcessor;
    }

    protected override Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken)
    {
        return _transactionProcessor.ProcessTransaction(transaction, cancellationToken);
    }

    public static TransactionProcessorSubscriber<TTransactionProcessor> Create(IServiceProvider serviceProvider,
        TTransactionProcessor transactionProcessor, bool testMode)
    {
        return ActivatorUtilities.CreateInstance<TransactionProcessorSubscriber<TTransactionProcessor>>(
            serviceProvider, transactionProcessor, testMode);
    }
}
