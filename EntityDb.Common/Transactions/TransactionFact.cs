using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Transactions;

namespace EntityDb.Common.Transactions
{
    internal sealed record TransactionFact<TEntity>(ulong SubversionNumber, IFact<TEntity> Fact) : ITransactionFact<TEntity>
    {
    }
}
