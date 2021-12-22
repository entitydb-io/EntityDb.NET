using EntityDb.MongoDb.Transactions;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.Extensions
{
    internal static class IMongoDbTransactionRepositoryFactoryExtensions
    {
        [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
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
