using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EntityDb.MongoDb.Provisioner.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddAutoProvisionMongoDbTransactions(
        this IServiceCollection serviceCollection, string databaseName,
        Func<IConfiguration, string> getConnectionString, bool testMode = false)
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
                    .UseTestMode(testMode)
                    .UseAutoProvisioning(serviceProvider);
            }
        );
    }
}
