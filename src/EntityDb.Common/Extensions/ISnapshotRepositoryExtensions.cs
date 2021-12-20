using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Extensions
{
    internal static class ISnapshotRepositoryExtensions
    {
        public static ISnapshotRepository<TEntity> UseTryCatch<TEntity>
        (
            this ISnapshotRepository<TEntity> snapshotRepository,
            ILogger logger
        )
        {
            return new TryCatchSnapshotRepository<TEntity>(snapshotRepository, logger);
        }
    }
}
