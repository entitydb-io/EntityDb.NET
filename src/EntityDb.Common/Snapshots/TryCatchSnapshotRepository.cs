using EntityDb.Abstractions.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal sealed class TryCatchSnapshotRepository<TSnapshot> : SnapshotRepositoryWrapper<TSnapshot>
{
    private readonly ILogger<TryCatchSnapshotRepository<TSnapshot>> _logger;

    public TryCatchSnapshotRepository
    (
        ISnapshotRepository<TSnapshot> snapshotRepository,
        ILogger<TryCatchSnapshotRepository<TSnapshot>> logger
    )
        : base(snapshotRepository)
    {
        _logger = logger;
    }

    protected override async Task<TSnapshot?> WrapQuery(Task<TSnapshot?> task)
    {
        try
        {
            return await task;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The operation cannot be completed");

            return default;
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
            _logger.LogError(exception, "The operation cannot be completed");

            return default;
        }
    }

    public static ISnapshotRepository<TSnapshot> Create(IServiceProvider serviceProvider,
        ISnapshotRepository<TSnapshot> snapshotRepository)
    {
        return ActivatorUtilities.CreateInstance<TryCatchSnapshotRepository<TSnapshot>>(serviceProvider,
            snapshotRepository);
    }
}
