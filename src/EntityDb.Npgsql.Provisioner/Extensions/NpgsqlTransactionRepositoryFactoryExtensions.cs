using EntityDb.Npgsql.Provisioner.Transactions;
using EntityDb.Npgsql.Queries;
using EntityDb.SqlDb.Transactions;

namespace EntityDb.Npgsql.Provisioner.Extensions;

internal static class NpgsqlTransactionRepositoryFactoryExtensions
{
    public static ISqlDbTransactionRepositoryFactory<NpgsqlQueryOptions> UseAutoProvisioning(
        this ISqlDbTransactionRepositoryFactory<NpgsqlQueryOptions> npgsqlTransactionRepositoryFactory,
        IServiceProvider serviceProvider)
    {
        return AutoProvisionNpgsqlTransactionRepositoryFactory.Create(serviceProvider,
            npgsqlTransactionRepositoryFactory);
    }
}
