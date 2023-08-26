using EntityDb.EntityFramework.Snapshots;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.EntityFramework.Extensions;

internal static class EntityFrameworkSnapshotRepositoryFactoryExtensions
{
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> UseTestMode<TSnapshot>(
        this IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> entityFrameworkSnapshotRepositoyFactory,
        IServiceProvider serviceProvider,
        bool testMode)
    {
        return testMode
            ? TestModeEntityFrameworkSnapshotRepositoryFactory<TSnapshot>.Create(serviceProvider, entityFrameworkSnapshotRepositoyFactory)
            : entityFrameworkSnapshotRepositoyFactory;
    }
}
