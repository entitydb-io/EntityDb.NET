using EntityDb.Npgsql.Queries;
using EntityDb.Npgsql.Transactions;
using EntityDb.SqlDb.Transactions;

namespace EntityDb.Npgsql.Extensions;

internal static class NpgsqlTransactionRepositoryFactoryExtensions
{
    public static ISqlDbTransactionRepositoryFactory<NpgsqlQueryOptions> UseAutoProvision(
        this ISqlDbTransactionRepositoryFactory<NpgsqlQueryOptions> npgsqlTransactionRepositoryFactory,
        IServiceProvider serviceProvider, bool autoProvision)
    {
        return autoProvision
            ? AutoProvisionNpgsqlTransactionRepositoryFactory.Create(serviceProvider, npgsqlTransactionRepositoryFactory)
            : npgsqlTransactionRepositoryFactory;
    }
}
