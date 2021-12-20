using EntityDb.MongoDb.Provisioner.Transactions;
using EntityDb.MongoDb.Transactions;

namespace EntityDb.MongoDb.Provisioner.Extensions
{
    internal static class IMongoDbTransactionRepositoryFactoryExtensions
    {
        public static IMongoDbTransactionRepositoryFactory<TEntity> UseAutoProvisioning<TEntity>(this IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory)
        {
            return new AutoProvisionMongoDbTransactionRepositoryFactory<TEntity>(mongoDbTransactionRepositoryFactory);
        }
    }
}
