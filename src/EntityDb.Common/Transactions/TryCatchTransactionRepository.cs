using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal sealed class TryCatchTransactionRepository : TransactionRepositoryWrapper
{
    private readonly ILogger<TryCatchTransactionRepository> _logger;

    public TryCatchTransactionRepository
    (
        ITransactionRepository transactionRepository,
        ILogger<TryCatchTransactionRepository> logger
    ) : base(transactionRepository)
    {
        _logger = logger;
    }

    protected override async Task<T[]> WrapQuery<T>(Func<Task<T[]>> task)
    {
        using (_logger.BeginScope("TryCatchId: {TryCatchId}", Guid.NewGuid()))
        {
            try
            {
                return await task.Invoke();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "The operation cannot be completed");

                return Array.Empty<T>();
            }
        }
    }

    protected override async Task<bool> WrapCommand(Func<Task<bool>> task)
    {
        using (_logger.BeginScope("TryCatchId: {TryCatchId}", Guid.NewGuid()))
        {
            try
            {
                return await task.Invoke();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "The operation cannot be completed");

                return default;
            }
        }
    }

    public static ITransactionRepository Create(IServiceProvider serviceProvider,
        ITransactionRepository transactionRepository)
    {
        return ActivatorUtilities.CreateInstance<TryCatchTransactionRepository>(serviceProvider,
            transactionRepository);
    }
}
