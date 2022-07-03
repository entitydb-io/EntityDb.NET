using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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
    /// <param name="databaseName">The name of the MongoDB database.</param>
    /// <param name="getConnectionString">A function that retrieves the MongoDB connection string.</param>
    /// <param name="testMode">Modifies the behavior of the repository to accomodate tests.</param>
    public static void AddMongoDbTransactions(this IServiceCollection serviceCollection,
        string databaseName, Func<IConfiguration, string> getConnectionString,
        bool testMode = false)
    {
        serviceCollection.AddBsonDocumentEnvelopeService(true);

        serviceCollection.Add<ITransactionRepositoryFactory>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient,
            serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var connectionString = getConnectionString.Invoke(configuration);

                return MongoDbTransactionRepositoryFactory
                    .Create(serviceProvider, connectionString, databaseName)
                    .UseTestMode(testMode);
            }
        );
    }
}
