using EntityDb.Abstractions.Strategies;
using EntityDb.TestImplementations.Entities;

namespace EntityDb.TestImplementations.Strategies
{
    public class TransactionEntitySnapshottingStrategy : ISnapshottingStrategy<TransactionEntity>
    {
        public bool ShouldPutSnapshot(TransactionEntity? previousSnapshot, TransactionEntity nextSnapshot)
        {
            return true;
        }
    }
}
