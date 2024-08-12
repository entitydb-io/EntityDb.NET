using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.States;

internal static class MongoDbStateRepositoryFactoryExtensions
{
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static IMongoDbStateRepositoryFactory<TState> UseTestMode<TState>(
        this IMongoDbStateRepositoryFactory<TState> mongoDbStateRepositoryFactory,
        bool testMode)
    {
        return testMode
            ? new TestModeMongoDbStateRepositoryFactory<TState>(mongoDbStateRepositoryFactory)
            : mongoDbStateRepositoryFactory;
    }

    public static IMongoDbStateRepositoryFactory<TState> UseAutoProvision<TState>(
        this IMongoDbStateRepositoryFactory<TState> mongoDbStateRepositoryFactory,
        IServiceProvider serviceProvider,
        bool autoProvision)
    {
        return autoProvision
            ? AutoProvisionMongoDbStateRepositoryFactory<TState>.Create(serviceProvider,
                mongoDbStateRepositoryFactory)
            : mongoDbStateRepositoryFactory;
    }
}
