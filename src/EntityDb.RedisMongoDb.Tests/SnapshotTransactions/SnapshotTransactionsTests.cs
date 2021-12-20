using EntityDb.Common.Tests.SnapshotTransactions;
using System;

namespace EntityDb.RedisMongoDb.Tests.SnapshotTransactions
{
    public class SnapshotTransactionsTests : SnapshotTransactionsTestsBase<Startup>
    {
        public SnapshotTransactionsTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
