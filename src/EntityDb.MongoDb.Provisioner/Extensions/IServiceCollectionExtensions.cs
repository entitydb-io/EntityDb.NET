using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EntityDb.MongoDb.Provisioner.Extensions
{
    internal static class IServiceCollectionExtensions
    {
        public static void AddAutoProvisionTestModeMongoDbTransactions<TEntity>(
            this IServiceCollection serviceCollection, string databaseName,
            Func<IConfiguration, string> getConnectionString, bool testMode = false)
        {
            serviceCollection.Add<ITransactionRepositoryFactory<TEntity>>
            (
                testMode ? ServiceLifetime.Singleton : ServiceLifetime.Scoped,
                serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                    var connectionString = getConnectionString.Invoke(configuration);

                    return MongoDbTransactionRepositoryFactory<TEntity>
                        .Create(serviceProvider, connectionString, databaseName)
                        .UseTestMode(testMode)
                        .UseAutoProvisioning();
                }
            );
        }
    }
}
