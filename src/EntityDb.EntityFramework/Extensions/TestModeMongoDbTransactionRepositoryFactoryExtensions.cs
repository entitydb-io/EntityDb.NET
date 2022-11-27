using EntityDb.EntityFramework.Snapshots;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.EntityFramework.Extensions;

internal static class EntityFrameworkSnapshotRepositoryFactoryExtensions
{

    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> UseTestMode<TSnapshot>(
        this IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> entityFrameworkSnapshotRepositoyFactory,
        bool testMode)
    {
        return testMode
            ? new TestModeEntityFrameworkSnapshotRepositoryFactory<TSnapshot>(entityFrameworkSnapshotRepositoyFactory)
            : entityFrameworkSnapshotRepositoyFactory;
    }
}
