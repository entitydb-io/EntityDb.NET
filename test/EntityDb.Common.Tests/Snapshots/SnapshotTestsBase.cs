using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests.Implementations.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Shouldly;
using System;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using Xunit;

namespace EntityDb.Common.Tests.Snapshots;

public abstract class SnapshotTestsBase<TStartup> : TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    protected SnapshotTestsBase(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    [Fact]
    public async Task GivenEmptySnapshotRepository_WhenGoingThroughFullCycle_ThenOriginalMatchesSnapshot()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var expectedSnapshot = new TransactionEntity { VersionNumber = new VersionNumber(300) };

        var snapshotId = Id.NewId();

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TransactionEntity>>()
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var snapshotInserted = await snapshotRepository.PutSnapshot(snapshotId, expectedSnapshot);

        var actualSnapshot = await snapshotRepository.GetSnapshot(snapshotId);

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

        var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);

        loggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<Type>()))
            .Returns(loggerMock.Object);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactoryMock.Object);
        });

        var snapshot = new TransactionEntity();

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TransactionEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnly);
        
        // ACT

        var inserted = await snapshotRepository.PutSnapshot(default, snapshot);

        // ASSERT

        inserted.ShouldBeFalse();

        loggerMock.Verify();
    }

    [Fact]
    public async Task GivenSnapshotInserted_WhenReadingInVariousReadModes_ThenReturnSameSnapshot()
    {
        // ARRANGE

        var snapshotId = Id.NewId();

        var expectedSnapshot = new TransactionEntity(new VersionNumber(5000));

        using var serviceScope = CreateServiceScope();

        await using var writeSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TransactionEntity>>()
            .CreateRepository(TestSessionOptions.Write);
        
        await using var readOnlySnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TransactionEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnly);
        
        await using var readOnlySecondaryPreferredSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TransactionEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnlySecondaryPreferred);
        
        var inserted = await writeSnapshotRepository.PutSnapshot(snapshotId, expectedSnapshot);

        // ARRANGE ASSERTIONS

        inserted.ShouldBeTrue();

        // ACT

        var readOnlySnapshot = await readOnlySnapshotRepository.GetSnapshot(snapshotId);

        var readOnlySecondaryPreferredSnapshot = await readOnlySecondaryPreferredSnapshotRepository.GetSnapshot(snapshotId);

        // ASSERT

        readOnlySnapshot.ShouldBeEquivalentTo(expectedSnapshot);
        readOnlySecondaryPreferredSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
    }
}