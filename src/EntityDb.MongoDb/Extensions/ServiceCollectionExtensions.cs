using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Documents.Envelopes;
using EntityDb.MongoDb.Snapshots;
using EntityDb.MongoDb.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.MongoDb.Extensions;

/// <summary>
///     Extensions for service collections.
/// </summary>
public static class ServiceCollectionExtensions
{
    internal static void AddBsonDocumentEnvelopeService(this IServiceCollection serviceCollection,
        bool removeTypeDiscriminatorProperty)
    {
        serviceCollection.AddSingleton(serviceProvider =>
            MongoDbEnvelopeService.Create(serviceProvider, removeTypeDiscriminatorProperty));
    }

    /// <summary>
    ///     Adds a production-ready implementation of <see cref="ISourceRepositoryFactory" /> to a service
    ///     collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="testMode">Modifies the behavior of the repository to accomodate tests.</param>
    /// <param name="autoProvision">Modifies the behavior of the repository to auto-provision collections.</param>
    public static void AddMongoDbSources(this IServiceCollection serviceCollection,
        bool testMode = false, bool autoProvision = false)
    {
        serviceCollection.AddBsonDocumentEnvelopeService(true);

        serviceCollection.Add<MongoDbSourceRepositoryFactory>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient
        );

        serviceCollection.Add<ISourceRepositoryFactory>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient,
            serviceProvider => serviceProvider
                .GetRequiredService<MongoDbSourceRepositoryFactory>()
                .UseTestMode(testMode)
                .UseAutoProvision(serviceProvider, autoProvision)
        );
    }

    /// <summary>
    ///     Adds a production-ready implementation of <see cref="ISourceRepositoryFactory" /> to a service
    ///     collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="testMode">Modifies the behavior of the repository to accomodate tests.</param>
    /// <param name="autoProvision">Modifies the behavior of the repository to auto-provision collections.</param>
    public static void AddMongoDbSnapshots<TSnapshot>(this IServiceCollection serviceCollection,
        bool testMode = false, bool autoProvision = false)
        where TSnapshot : notnull
    {
        serviceCollection.AddBsonDocumentEnvelopeService(true);

        serviceCollection.Add<MongoDbSnapshotRepositoryFactory<TSnapshot>>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient
        );

        serviceCollection.Add<ISnapshotRepositoryFactory<TSnapshot>>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient,
            serviceProvider => serviceProvider
                .GetRequiredService<MongoDbSnapshotRepositoryFactory<TSnapshot>>()
                .UseTestMode(testMode)
                .UseAutoProvision(serviceProvider, autoProvision)
        );
    }
}
