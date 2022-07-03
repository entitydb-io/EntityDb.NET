using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.Extensions;

/// <summary>
///     Extensions for service collections.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "All of the tests in this project are using the auto-provisioning variant.")]
public static class ServiceCollectionExtensions
{
    internal static void AddBsonDocumentEnvelopeService(this IServiceCollection serviceCollection,
        bool removeTypeDiscriminatorProperty)
    {
        serviceCollection.AddSingleton(serviceProvider =>
            BsonDocumentEnvelopeService.Create(serviceProvider, removeTypeDiscriminatorProperty));
    }

    /// <summary>
    ///     Adds a production-ready implementation of <see cref="ITransactionRepositoryFactory" /> to a service
    ///     collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="testMode">Modifies the behavior of the repository to accomodate tests.</param>
    public static void AddMongoDbTransactions(this IServiceCollection serviceCollection,
        bool testMode = false)
    {
        serviceCollection.AddBsonDocumentEnvelopeService(true);

        serviceCollection.Add<MongoDbTransactionRepositoryFactory>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient
        );

        serviceCollection.Add<ITransactionRepositoryFactory>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient,
            serviceProvider => serviceProvider
                .GetRequiredService<MongoDbTransactionRepositoryFactory>()
                .UseTestMode(testMode)
        );
    }
}
