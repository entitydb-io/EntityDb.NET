using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions.Subscribers.Processors;
using EntityDb.Common.Transactions.Subscribers.Reprocessors;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Common.Transactions.Subscribers.ReprocessorQueues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal class BufferBlockTransactionReprocessorQueue : TransactionReprocessorQueueBase, ITransactionReprocessorQueue
{
    private readonly BufferBlock<IReprocessTransactionsRequest> _bufferBlock = new();
    private readonly ILogger<BufferBlockTransactionReprocessorQueue> _logger;
    private readonly ITransactionRepositoryFactory _transactionRepositoryFactory;
    private readonly IEnumerable<ITransactionProcessor> _transactionProcessors;

    public BufferBlockTransactionReprocessorQueue(ILogger<BufferBlockTransactionReprocessorQueue> logger, ITransactionRepositoryFactory transactionRepositoryFactory, IEnumerable<ITransactionProcessor> transactionProcessors) : base(logger, transactionRepositoryFactory, transactionProcessors)
    {
        _logger = logger;
        _transactionRepositoryFactory = transactionRepositoryFactory;
        _transactionProcessors = transactionProcessors;
    }

    public void Enqueue(IReprocessTransactionsRequest reprocessTransactionsRequest)
    {
        _bufferBlock.Post(reprocessTransactionsRequest);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _bufferBlock.OutputAvailableAsync(stoppingToken))
        {
            var reprocessTransactionsRequest = await _bufferBlock.ReceiveAsync(stoppingToken);

            using var _ = _logger.BeginScope(reprocessTransactionsRequest.ToLogScopeState());

            try
            {
                _logger.LogDebug("Started reprocessing transactions");

                await Process(reprocessTransactionsRequest, stoppingToken);

                _logger.LogDebug("Finished reprocessing transactions");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to reprocess transactions");
            }
        }
    }
}
