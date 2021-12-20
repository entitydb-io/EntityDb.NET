using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Extensions
{
    internal static class ISnapshotRepositoryFactoryExtensions
    {
        public static ISnapshotRepositoryFactory<TEntity> UseTestMode<TEntity>
        (
            this ISnapshotRepositoryFactory<TEntity> snapshotRepositoryFactory,
            SnapshotTestMode? snapshotTestMode
        )
        {
            if (snapshotTestMode.HasValue)
            {
                return new TestModeSnapshotRepositoryFactory<TEntity>(snapshotRepositoryFactory, snapshotTestMode.Value);
            }

            return snapshotRepositoryFactory;
        }
    }
}
