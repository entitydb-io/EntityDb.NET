using EntityDb.MongoDb.Transactions;

namespace EntityDb.MongoDb.Extensions
{
    internal static class IMongoDbTransactionRepositoryFactoryExtensions
    {
        public static IMongoDbTransactionRepositoryFactory<TEntity> UseTestMode<TEntity>(
            this IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory,
            bool testMode)
        {
            if (testMode)
            {
                return new TestModeMongoDbTransactionRepositoryFactory<TEntity>(mongoDbTransactionRepositoryFactory);
            }

            return mongoDbTransactionRepositoryFactory;
        }
    }
}
