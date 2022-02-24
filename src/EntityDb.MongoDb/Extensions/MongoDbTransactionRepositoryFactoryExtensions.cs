using EntityDb.MongoDb.Transactions;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.Extensions;

internal static class MongoDbTransactionRepositoryFactoryExtensions
{
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static IMongoDbTransactionRepositoryFactory<TEntity> UseTestMode<TEntity>(
        this IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory,
        bool testMode)
    {
        return testMode
            ? new TestModeMongoDbTransactionRepositoryFactory<TEntity>(mongoDbTransactionRepositoryFactory)
            : mongoDbTransactionRepositoryFactory;
    }
}
