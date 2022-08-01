using EntityDb.SqlDb.Transactions;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.SqlDb.Extensions;

internal static class SqlDbTransactionRepositoryFactoryExtensions
{
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static ISqlDbTransactionRepositoryFactory<TOptions> UseTestMode<TOptions>(
        this ISqlDbTransactionRepositoryFactory<TOptions> mongoDbTransactionRepositoryFactory,
        bool testMode)
        where TOptions : class
    {
        return testMode
            ? new TestModeSqlDbTransactionRepositoryFactory<TOptions>(mongoDbTransactionRepositoryFactory)
            : mongoDbTransactionRepositoryFactory;
    }
}
