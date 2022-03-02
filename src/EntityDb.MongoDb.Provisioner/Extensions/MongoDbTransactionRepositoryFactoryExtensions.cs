using EntityDb.MongoDb.Provisioner.Transactions;
using EntityDb.MongoDb.Transactions;

namespace EntityDb.MongoDb.Provisioner.Extensions;

internal static class MongoDbTransactionRepositoryFactoryExtensions
{
    public static IMongoDbTransactionRepositoryFactory UseAutoProvisioning(
        this IMongoDbTransactionRepositoryFactory mongoDbTransactionRepositoryFactory)
    {
        return new AutoProvisionMongoDbTransactionRepositoryFactory(mongoDbTransactionRepositoryFactory);
    }
}
