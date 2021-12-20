using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Transactions;

namespace EntityDb.MongoDb.Extensions
{
    internal static class ITransactionRepositoryExtensions
    {
        public static ITransactionRepository<TEntity> UseTestMode<TEntity>
        (
            this ITransactionRepository<TEntity> transactionRepository,
            TestModeTransactionManager testModeTransactionManager,
            TransactionTestMode transactionTestMode
        )
        {
            return new TestModeMongoDbTransactionRepository<TEntity>(transactionRepository, testModeTransactionManager, transactionTestMode);
        }
    }
}
