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
using Microsoft.Extensions.Logging;
using Xunit;

namespace EntityDb.Common.Tests.Snapshots;

public sealed class SnapshotTests : TestsBase<Startup>
{
    public SnapshotTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    public async Task GivenEmptySnapshotRepository_WhenGoingThroughFullCycle_ThenOriginalMatchesSnapshot(SnapshotsAdder entitySnapshotsAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entitySnapshotsAdder.Add(serviceCollection);
        });

        var expectedSnapshot = new TestEntity { VersionNumber = new VersionNumber(300) };

        var snapshotId = Id.NewId();

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TestEntity>>();
        
        await using var snapshotRepository = await snapshotRepositoryFactory
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var snapshotInserted = await snapshotRepository.PutSnapshot(snapshotId, expectedSnapshot);

        var actualSnapshot = await snapshotRepository.GetSnapshot(snapshotId);

        // ASSERT

        snapshotInserted.ShouldBeTrue();

        actualSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    public async Task GivenReadOnlyMode_WhenPuttingSnapshot_ThenCannotWriteInReadOnlyModeExceptionIsLogged(SnapshotsAdder entitySnapshotsAdder)
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<CannotWriteInReadOnlyModeException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entitySnapshotsAdder.Add(serviceCollection);
            
            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var snapshot = new TestEntity();

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TestEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnly);
        
        // ACT

        var inserted = await snapshotRepository.PutSnapshot(default, snapshot);

        // ASSERT

        inserted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Once());
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    public async Task GivenSnapshotInserted_WhenReadingInVariousReadModes_ThenReturnSameSnapshot(SnapshotsAdder entitySnapshotsAdder)
    {
        // ARRANGE

        var snapshotId = Id.NewId();

        var expectedSnapshot = new TestEntity(new VersionNumber(5000));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entitySnapshotsAdder.Add(serviceCollection);
        });
        
        await using var writeSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TestEntity>>()
            .CreateRepository(TestSessionOptions.Write);
        
        await using var readOnlySnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TestEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnly);
        
        await using var readOnlySecondaryPreferredSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TestEntity>>()
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