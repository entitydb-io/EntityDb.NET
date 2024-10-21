using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.Processors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Common.Transactions.Subscribers.ProcessorQueues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal class BufferBlockTransactionQueue<TTransactionProcessor> : BackgroundService, ITransactionProcessorQueue<TTransactionProcessor>
    where TTransactionProcessor : ITransactionProcessor
{
    private readonly BufferBlock<ITransaction> _foregroundQueue = new();
    private readonly BufferBlock<ITransaction> _backgroundQueue = new();
    private readonly IDisposable _link;
    private readonly ILogger<BufferBlockTransactionQueue<TTransactionProcessor>> _logger;
    private readonly TTransactionProcessor _transactionProcessor;

    public BufferBlockTransactionQueue(ILogger<BufferBlockTransactionQueue<TTransactionProcessor>> logger, TTransactionProcessor transactionProcessor)
    {
        _link = _foregroundQueue.LinkTo(_backgroundQueue);
        
        _logger = logger;
        _transactionProcessor = transactionProcessor;
    }

    public void Enqueue(ITransaction transaction)
    {
        _logger.LogInformation("Enqueueing Transaction {TransactionId} to Transaction Queue.", transaction.Id.Value);
        
        var enqueued = _foregroundQueue.Post(transaction);

        _logger.LogInformation("{Enqueued} Transaction {TransactionId} to Transaction Queue.", enqueued, transaction.Id.Value);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _backgroundQueue.OutputAvailableAsync(stoppingToken))
        {
            try
            {
                var transaction = await _backgroundQueue.ReceiveAsync(stoppingToken);

                await _transactionProcessor.ProcessTransaction(transaction, stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while processing transaction");
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        _link.Dispose();
    }
}
