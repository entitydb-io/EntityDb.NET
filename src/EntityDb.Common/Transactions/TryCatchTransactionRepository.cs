using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Transactions;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal sealed class TryCatchTransactionRepository<TEntity> : TransactionRepositoryWrapper<TEntity>
{
    private readonly ILogger _logger;

    public TryCatchTransactionRepository(ITransactionRepository<TEntity> transactionRepository, ILogger logger) :
        base(transactionRepository)
    {
        _logger = logger;
    }

    protected override async Task<T[]> WrapQuery<T>(Task<T[]> task)
    {
        try
        {
            return await task;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The operation cannot be completed.");

            return Array.Empty<T>();
        }
    }

    protected override async Task<bool> WrapCommand(Task<bool> task)
    {
        try
        {
            return await task;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The operation cannot be completed.");

            return default;
        }
    }
}
