using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Npgsql.Converters;
using EntityDb.Npgsql.Extensions;
using EntityDb.Npgsql.Queries;
using EntityDb.Npgsql.Transactions;
using EntityDb.SqlDb.Converters;
using EntityDb.SqlDb.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Npgsql.Provisioner.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddAutoProvisionNpgsqlTransactions(
        this IServiceCollection serviceCollection, bool testMode = false)
    {
        serviceCollection.AddSqlDbEnvelopeService();

        serviceCollection.Add<ISqlConverter<NpgsqlQueryOptions>, NpgsqlConverter>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient
        );

        serviceCollection.Add<NpgsqlTransactionRepositoryFactory>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient
        );

        serviceCollection.Add<ITransactionRepositoryFactory>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient,
            serviceProvider => serviceProvider
                .GetRequiredService<NpgsqlTransactionRepositoryFactory>()
                .UseTestMode(testMode)
                .UseAutoProvisioning(serviceProvider)
        );
    }
}
