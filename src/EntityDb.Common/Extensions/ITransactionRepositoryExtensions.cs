using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;

namespace EntityDb.Common.Extensions
{
    internal static class ITransactionRepositoryExtensions
    {
        public static ITransactionRepository<TEntity> WithTryCatch<TEntity>
        (
            this ITransactionRepository<TEntity> transactionRepository,
            ILogger logger
        )
        {
            return new TryCatchTransactionRepository<TEntity>(transactionRepository, logger);
        }
    }
}
