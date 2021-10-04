using EntityDb.Abstractions.Snapshots;
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

            TransactionEntity? expectedSnapshot = new TransactionEntity { VersionNumber = 300 };

            Guid entityId = Guid.NewGuid();

            await using ISnapshotRepository<TransactionEntity>? snapshotRepository =
                await _serviceProvider.CreateSnapshotRepository<TransactionEntity>(new SnapshotSessionOptions());

            // ACT

            bool snapshotInserted = await snapshotRepository.PutSnapshot(entityId, expectedSnapshot);

            TransactionEntity? actualSnapshot = await snapshotRepository.GetSnapshot(entityId);

            // ASSERT

            snapshotInserted.ShouldBeTrue();

            actualSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
        }
    }
}
