using EntityDb.Abstractions.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Sources;

internal sealed class TryCatchSourceRepository : SourceRepositoryWrapper
{
    private readonly ILogger<TryCatchSourceRepository> _logger;

    public TryCatchSourceRepository
    (
        ISourceRepository sourceRepository,
        ILogger<TryCatchSourceRepository> logger
    ) : base(sourceRepository)
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

    public static ISourceRepository Create(IServiceProvider serviceProvider,
        ISourceRepository sourceRepository)
    {
        return ActivatorUtilities.CreateInstance<TryCatchSourceRepository>(serviceProvider,
            sourceRepository);
    }
}
