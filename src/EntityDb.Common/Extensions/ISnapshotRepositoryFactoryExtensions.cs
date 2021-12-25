using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Snapshots;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Common.Extensions
{
    internal static class ISnapshotRepositoryFactoryExtensions
    {
        [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
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
