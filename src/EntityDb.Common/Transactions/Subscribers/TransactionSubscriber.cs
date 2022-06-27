using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Common.Transactions.Subscribers;

/// <summary>
///     Represents an asynchronous subscription to transactions.
/// </summary>
public abstract class TransactionSubscriber : BackgroundService, ITransactionSubscriber
{
    private readonly BufferBlock<ITransaction> _transactionQueue = new();
    private readonly bool _testMode;

    /// <summary>
    ///     Constructs a new instance of <see cref="TransactionSubscriber" />.
    /// </summary>
    /// <param name="testMode">If <c>true</c> then the task will be synchronously awaited before returning.</param>
    protected TransactionSubscriber(bool testMode)
    {
        _testMode = testMode;
    }

    /// <inheritdoc />
    public void Notify(ITransaction transaction)
    {
        if (_testMode)
        {
            Task.Run(() => ProcessTransaction(transaction, default)).Wait();
        }
        else
        {
            _transactionQueue.Post(transaction);
        }
    }

    /// <ignore/>
    [Obsolete("Please implement ProcessTransaction(...) instead. This method will be removed at a later date.")]
    protected virtual Task NotifyAsync(ITransaction transaction)
        => ProcessTransaction(transaction, default);

    /// <summary>
    ///     Processes a transaction.
    /// </summary>
    /// <param name="transaction">The transaction</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task.</returns>
    protected abstract Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _transactionQueue.OutputAvailableAsync(stoppingToken))
        {
            var transaction = await _transactionQueue.ReceiveAsync(stoppingToken);

            await ProcessTransaction(transaction, stoppingToken);
        }
    }
}
