using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Snapshots;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Extensions;

internal static class SnapshotRepositoryExtensions
{
    public static ISnapshotRepository<TEntity> UseTryCatch<TEntity>
    (
        this ISnapshotRepository<TEntity> snapshotRepository,
        ILogger logger
    )
    {
        return new TryCatchSnapshotRepository<TEntity>(snapshotRepository, logger);
    }

    public static async Task<TEntity?> GetSnapshotOrDefault<TEntity>(this ISnapshotRepository<TEntity>? snapshotRepository, Guid entityId)
    {
        if (snapshotRepository != null)
        {
            return await snapshotRepository.GetSnapshot(entityId);
        }

        return default;
    }
}
