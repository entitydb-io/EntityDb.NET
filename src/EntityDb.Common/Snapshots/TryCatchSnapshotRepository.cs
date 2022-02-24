using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal sealed class TryCatchSnapshotRepository<TEntity> : SnapshotRepositoryWrapper<TEntity>
{
    private readonly ILogger _logger;

    public TryCatchSnapshotRepository
    (
        ISnapshotRepository<TEntity> snapshotRepository,
        ILogger logger
    )
        : base(snapshotRepository)
    {
        _logger = logger;
    }

    protected override async Task<TEntity?> WrapQuery(Task<TEntity?> task)
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
