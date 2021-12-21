using EntityDb.Common.Tests.Snapshots;
using System;

namespace EntityDb.Redis.Tests.Snapshots
{
    public class SnapshotTests : SnapshotTestsBase<Startup>
    {
        public SnapshotTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }
    }
}
