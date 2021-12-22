using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Snapshots;
using EntityDb.TestImplementations.Entities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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

        [Fact]
        public async Task GivenReadOnlyMode_WhenPuttingSnapshot_ThenCannotWriteInReadOnlyModeExceptionIsLogged()
        {
            // ARRANGE

            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

            loggerMock
                .Setup(logger => logger.LogError(It.IsAny<CannotWriteInReadOnlyModeException>(), It.IsAny<string>()))
                .Verifiable();

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.Configure<SnapshotSessionOptions>("TestReadOnlyWithLoggerOverride", options =>
                {
                    options.LoggerOverride = loggerMock.Object;
                    options.ReadOnly = true;
                    options.SecondaryPreferred = true;
                });
            });

            var snapshot = new TransactionEntity();

            await using var snapshotRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ISnapshotRepositoryFactory<TransactionEntity>>()
                .CreateRepository("TestReadOnlyWithLoggerOverride");

            // ACT

            var inserted = await snapshotRepository.PutSnapshot(default, snapshot);

            // ASSERT

            inserted.ShouldBeFalse();

            loggerMock.Verify();
        }
    }
}
