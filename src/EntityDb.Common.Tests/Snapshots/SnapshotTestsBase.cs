using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.TestImplementations.Entities;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.Snapshots
{
    public abstract class SnapshotTestsBase
    {
        private readonly IServiceProvider _serviceProvider;

        public SnapshotTestsBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Fact]
        public async Task GivenEmptySnapshotRepository_WhenGoingThroughFullCycle_ThenOriginalMatchesSnapshot()
        {
            // ARRANGE

            var expectedSnapshot = new TransactionEntity { VersionNumber = 300 };

            var entityId = Guid.NewGuid();

            await using var snapshotRepository =
                await _serviceProvider.CreateSnapshotRepository<TransactionEntity>(new SnapshotSessionOptions());

            // ACT

            var snapshotInserted = await snapshotRepository.PutSnapshot(entityId, expectedSnapshot);

            var actualSnapshot = await snapshotRepository.GetSnapshot(entityId);

            // ASSERT

            snapshotInserted.ShouldBeTrue();

            actualSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
        }
    }
}
