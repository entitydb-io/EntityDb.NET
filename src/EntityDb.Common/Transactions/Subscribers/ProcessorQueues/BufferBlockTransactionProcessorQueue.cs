using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.Processors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Common.Transactions.Subscribers.ProcessorQueues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal class BufferBlockTransactionProcessorQueue<TTransactionProcessor> : BackgroundService, ITransactionProcessorQueue<TTransactionProcessor>
    where TTransactionProcessor : ITransactionProcessor
{
    private readonly BufferBlock<ITransaction> _transactionQueue = new();
    private readonly ILogger<BufferBlockTransactionProcessorQueue<TTransactionProcessor>> _logger;
    private readonly TTransactionProcessor _transactionProcessor;

    public BufferBlockTransactionProcessorQueue(ILogger<BufferBlockTransactionProcessorQueue<TTransactionProcessor>> logger, TTransactionProcessor transactionProcessor)
    {
        _logger = logger;
        _transactionProcessor = transactionProcessor;
    }

    public void Enqueue(ITransaction transaction)
    {
        _transactionQueue.Post(transaction);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _transactionQueue.OutputAvailableAsync(stoppingToken))
        {
            var transaction = await _transactionQueue.ReceiveAsync(stoppingToken);

            using var _ = _logger.BeginScope(new KeyValuePair<string, object>[]
            {
                new("TransactionId", transaction.Id.Value)
            });

            try
            {
                _logger.LogDebug("Started processing transaction");

                await _transactionProcessor.ProcessTransaction(transaction, stoppingToken);

                _logger.LogDebug("Finished processing transaction");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while processing transaction");
            }
        }
    }
}
