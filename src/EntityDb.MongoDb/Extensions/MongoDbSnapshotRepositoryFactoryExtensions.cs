using EntityDb.MongoDb.Snapshots;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.Extensions;

internal static class MongoDbSnapshotRepositoryFactoryExtensions
{
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static IMongoDbSnapshotRepositoryFactory<TSnapshot> UseTestMode<TSnapshot>(
        this IMongoDbSnapshotRepositoryFactory<TSnapshot> mongoDbSnapshotRepositoryFactory,
        bool testMode)
    {
        return testMode
            ? new TestModeMongoDbSnapshotRepositoryFactory<TSnapshot>(mongoDbSnapshotRepositoryFactory)
            : mongoDbSnapshotRepositoryFactory;
    }

    public static IMongoDbSnapshotRepositoryFactory<TSnapshot> UseAutoProvision<TSnapshot>(
        this IMongoDbSnapshotRepositoryFactory<TSnapshot> mongoDbSnapshotRepositoryFactory,
        IServiceProvider serviceProvider,
        bool autoProvision)
    {
        return autoProvision
            ? AutoProvisionMongoDbSnapshotRepositoryFactory<TSnapshot>.Create(serviceProvider, mongoDbSnapshotRepositoryFactory)
            : mongoDbSnapshotRepositoryFactory;
    }
}
