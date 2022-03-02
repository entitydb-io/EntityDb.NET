using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;

namespace EntityDb.Common.Extensions;

internal static class TransactionRepositoryExtensions
{
    public static ITransactionRepository UseTryCatch
    (
        this ITransactionRepository transactionRepository,
        ILogger logger
    )
    {
        return new TryCatchTransactionRepository(transactionRepository, logger);
    }
}
