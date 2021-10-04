using EntityDb.Abstractions.Facts;
using EntityDb.TestImplementations.Entities;

namespace EntityDb.TestImplementations.Facts
{
    public record NothingDone : IFact<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity;
        }
    }
}
