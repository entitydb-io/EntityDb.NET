using EntityDb.Abstractions.Snapshots;
using EntityDb.TestImplementations.Entities;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.Snapshots
{
    public abstract class SnapshotTestsBase<TStartup> : TestsBase<TStartup>
        where TStartup : IStartup, new()
    {
        public SnapshotTestsBase(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }

        [Fact]
        public async Task GivenEmptySnapshotRepository_WhenGoingThroughFullCycle_ThenOriginalMatchesSnapshot()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var snapshotRepositoryFactory = serviceScope.ServiceProvider
                .GetRequiredService<ISnapshotRepositoryFactory<TransactionEntity>>();

            var expectedSnapshot = new TransactionEntity { VersionNumber = 300 };

            var entityId = Guid.NewGuid();

            await using var snapshotRepository =
                await snapshotRepositoryFactory.CreateRepository("TestWrite");

            // ACT

            var snapshotInserted = await snapshotRepository.PutSnapshot(entityId, expectedSnapshot);

            var actualSnapshot = await snapshotRepository.GetSnapshot(entityId);

            // ASSERT

            snapshotInserted.ShouldBeTrue();

            actualSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
        }
    }
}
