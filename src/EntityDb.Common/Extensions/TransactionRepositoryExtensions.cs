using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.Logging;

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
