using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Transactions;

namespace EntityDb.MongoDb.Extensions
{
    internal static class IMongoDbTransactionRepositoryFactoryExtensions
    {
        public static IMongoDbTransactionRepositoryFactory<TEntity> UseTestMode<TEntity>(this IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory, TransactionTestMode? transactionTestMode)
        {
            if (transactionTestMode.HasValue)
            {
                return new TestModeMongoDbTransactionRepositoryFactory<TEntity>(mongoDbTransactionRepositoryFactory, transactionTestMode.Value);
            }

            return mongoDbTransactionRepositoryFactory;
        }
    }
}
