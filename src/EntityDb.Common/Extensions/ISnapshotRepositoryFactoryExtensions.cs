using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Extensions
{
    internal static class ISnapshotRepositoryFactoryExtensions
    {
        public static ISnapshotRepositoryFactory<TEntity> UseTestMode<TEntity>
        (
            this ISnapshotRepositoryFactory<TEntity> snapshotRepositoryFactory,
            bool testMode
        )
        {
            if (testMode)
            {
                return new TestModeSnapshotRepositoryFactory<TEntity>(snapshotRepositoryFactory);
            }

            return snapshotRepositoryFactory;
        }
    }
}
