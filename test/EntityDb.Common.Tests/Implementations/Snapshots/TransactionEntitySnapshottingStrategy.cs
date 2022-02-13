using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Tests.Implementations.Entities;

namespace EntityDb.Common.Tests.Implementations.Snapshots
{
    public class TransactionEntitySnapshotStrategy : ISnapshotStrategy<TransactionEntity>
    {
        public bool ShouldPutSnapshot(TransactionEntity? previousSnapshot, TransactionEntity nextSnapshot)
        {
            return true;
        }
    }
}
