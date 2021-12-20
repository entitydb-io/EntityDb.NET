using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Tests.Snapshots;
using EntityDb.TestImplementations.Entities;

namespace EntityDb.Redis.Tests.Snapshots
{
    public class SnapshotTests : SnapshotTestsBase
    {
        public SnapshotTests(ISnapshotRepositoryFactory<TransactionEntity> snapshotRepositoryFactory) : base(
            snapshotRepositoryFactory)
        {
        }
    }
}
