using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

    protected override async IAsyncEnumerable<T> WrapQuery<T>(Func<IAsyncEnumerable<T>> enumerable)
    {
        using (_logger.BeginScope("TryCatchId: {TryCatchId}", Guid.NewGuid()))
        {
            IAsyncEnumerator<T> enumerator;

            try
            {
                enumerator = enumerable.Invoke().GetAsyncEnumerator();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "The operation cannot be completed");

                yield break;
            }

            while (true)
            {
                try
                {
                    if (!await enumerator.MoveNextAsync())
                    {
                        yield break;
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "The operation cannot be completed");

                    yield break;
                }

                yield return enumerator.Current;
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
