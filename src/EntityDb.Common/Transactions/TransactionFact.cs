using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Transactions;

namespace EntityDb.Common.Transactions
{
    internal sealed record TransactionFact<TEntity> : ITransactionFact<TEntity>
    {
        public ulong SubversionNumber { get; init; }
        public IFact<TEntity> Fact { get; init; } = default!;
    }
}
