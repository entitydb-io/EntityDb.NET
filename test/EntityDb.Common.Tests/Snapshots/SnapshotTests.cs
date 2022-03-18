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
    [MemberData(nameof(AddSnapshots))]
    public async Task GivenEmptySnapshotRepository_WhenGoingThroughFullCycle_ThenOriginalMatchesSnapshot(SnapshotsAdder snapshotsAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotsAdder.Add(serviceCollection);
        });

        var expectedSnapshot = new TransactionEntity { VersionNumber = new VersionNumber(300) };

        var snapshotId = Id.NewId();

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TransactionEntity>>();
        
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
    [MemberData(nameof(AddSnapshots))]
    public async Task GivenReadOnlyMode_WhenPuttingSnapshot_ThenCannotWriteInReadOnlyModeExceptionIsLogged(SnapshotsAdder snapshotsAdder)
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<CannotWriteInReadOnlyModeException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotsAdder.Add(serviceCollection);
            
            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var snapshot = new TransactionEntity();

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TransactionEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnly);
        
        // ACT

        var inserted = await snapshotRepository.PutSnapshot(default, snapshot);

        // ASSERT

        inserted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Once());
    }

    [Theory]
    [MemberData(nameof(AddSnapshots))]
    public async Task GivenSnapshotInserted_WhenReadingInVariousReadModes_ThenReturnSameSnapshot(SnapshotsAdder snapshotsAdder)
    {
        // ARRANGE

        var snapshotId = Id.NewId();

        var expectedSnapshot = new TransactionEntity(new VersionNumber(5000));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotsAdder.Add(serviceCollection);
        });
        
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