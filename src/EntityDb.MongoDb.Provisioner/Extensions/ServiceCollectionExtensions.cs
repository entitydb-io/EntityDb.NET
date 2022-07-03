using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.MongoDb.Provisioner.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddAutoProvisionMongoDbTransactions(
        this IServiceCollection serviceCollection, bool testMode = false)
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
                .UseAutoProvisioning(serviceProvider)
        );
    }
}
