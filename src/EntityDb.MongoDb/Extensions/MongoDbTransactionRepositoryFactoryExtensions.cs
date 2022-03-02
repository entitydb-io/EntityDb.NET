using EntityDb.MongoDb.Transactions;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.Extensions;

internal static class MongoDbTransactionRepositoryFactoryExtensions
{
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static IMongoDbTransactionRepositoryFactory UseTestMode(
        this IMongoDbTransactionRepositoryFactory mongoDbTransactionRepositoryFactory,
        bool testMode)
    {
        return testMode
            ? new TestModeMongoDbTransactionRepositoryFactory(mongoDbTransactionRepositoryFactory)
            : mongoDbTransactionRepositoryFactory;
    }
}
