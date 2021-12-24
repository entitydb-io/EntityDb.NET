using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Tests.Implementations.Entities;

namespace EntityDb.Common.Tests.Implementations.Strategies
{
    public class TransactionEntitySnapshottingStrategy : ISnapshottingStrategy<TransactionEntity>
    {
        public bool ShouldPutSnapshot(TransactionEntity? previousSnapshot, TransactionEntity nextSnapshot)
        {
            return true;
        }
    }
}
