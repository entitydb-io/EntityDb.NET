using EntityDb.Abstractions.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.States;

internal sealed class TryCatchStateRepository<TState> : StateRepositoryWrapper<TState>
{
    private readonly ILogger<TryCatchStateRepository<TState>> _logger;

    public TryCatchStateRepository
    (
        ILogger<TryCatchStateRepository<TState>> logger,
        IStateRepository<TState> stateRepository
    )
        : base(stateRepository)
    {
        _logger = logger;
    }

    protected override async Task<TState?> WrapQuery(Func<Task<TState?>> task)
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

    public static IStateRepository<TState> Create(IServiceProvider serviceProvider,
        IStateRepository<TState> stateRepository)
    {
        return ActivatorUtilities.CreateInstance<TryCatchStateRepository<TState>>(serviceProvider,
            stateRepository);
    }
}
