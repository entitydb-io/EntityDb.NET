using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.Sources;

internal static class MongoDbSourceRepositoryFactoryExtensions
{
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static IMongoDbSourceRepositoryFactory UseTestMode(
        this IMongoDbSourceRepositoryFactory mongoDbSourceRepositoryFactory,
        bool testMode)
    {
        return testMode
            ? new TestModeMongoDbSourceRepositoryFactory(mongoDbSourceRepositoryFactory)
            : mongoDbSourceRepositoryFactory;
    }

    public static IMongoDbSourceRepositoryFactory UseAutoProvision(
        this IMongoDbSourceRepositoryFactory mongoDbSourceRepositoryFactory,
        IServiceProvider serviceProvider,
        bool autoProvision)
    {
        return autoProvision
            ? AutoProvisionMongoDbSourceRepositoryFactory.Create(serviceProvider, mongoDbSourceRepositoryFactory)
            : mongoDbSourceRepositoryFactory;
    }
}
