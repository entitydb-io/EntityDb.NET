﻿using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Snapshots;

[Collection(nameof(DatabaseContainerCollection))]
public sealed class SnapshotTests : TestsBase<Startup>
{
    public SnapshotTests(IServiceProvider startupServiceProvider, DatabaseContainerFixture databaseContainerFixture) : base(startupServiceProvider, databaseContainerFixture)
    {
    }

    private async Task
        Generic_GivenEmptySnapshotRepository_WhenSnapshotInsertedAndFetched_ThenInsertedMatchesFetched<TSnapshot>(
            SnapshotAdder snapshotAdder)
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        var snapshotId = Id.NewId();
        var expectedSnapshot = TSnapshot.Construct(snapshotId).WithVersionNumber(new VersionNumber(300));

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>();

        await using var snapshotRepository = await snapshotRepositoryFactory
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var snapshotInserted = await snapshotRepository.PutSnapshot(snapshotId, expectedSnapshot);

        var actualSnapshot = await snapshotRepository.GetSnapshotOrDefault(snapshotId);

        // ASSERT

        snapshotInserted.ShouldBeTrue();

        actualSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task GivenEmptySnapshotRepository_WhenSnapshotInsertedAndFetched_ThenInsertedMatchesFetched(
        SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }

    private async Task
        Generic_GivenEmptySnapshotRepository_WhenPuttingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged<
            TSnapshot>(SnapshotAdder snapshotAdder)
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<CannotWriteInReadOnlyModeException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var snapshot = TSnapshot.Construct(default).WithVersionNumber(new VersionNumber(300));

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.ReadOnly);

        // ACT

        var inserted = await snapshotRepository.PutSnapshot(default, snapshot);

        // ASSERT

        inserted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Once());
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task
        GivenEmptySnapshotRepository_WhenPuttingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged(
            SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }

    private async Task
        Generic_GivenInsertedSnapshotAsLatest_WhenSnapshotDeleted_ThenReturnNoSnapshot<TSnapshot>(
        SnapshotAdder snapshotAdder)
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        Pointer latestSnapshotPointer = Id.NewId();

        var snapshot = TSnapshot.Construct(latestSnapshotPointer.Id).WithVersionNumber(new VersionNumber(5000));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotAdder.AddDependencies.Invoke(serviceCollection);

            TSnapshot.ShouldRecordAsLatestLogic.Value = (_, _) => true;
        });

        await using var writeSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.Write);

        var inserted = await writeSnapshotRepository.PutSnapshot(latestSnapshotPointer, snapshot);

        // ARRANGE ASSERTIONS

        inserted.ShouldBeTrue();

        // ACT

        var deleted = await writeSnapshotRepository.DeleteSnapshots(new[] { latestSnapshotPointer });

        var finalSnapshot = await writeSnapshotRepository.GetSnapshotOrDefault(latestSnapshotPointer);

        // ASSERT

        deleted.ShouldBeTrue();

        finalSnapshot.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task GivenInsertedSnapshotAsLatest_WhenSnapshotDeleted_ThenReturnNoSnapshot(SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }

    private async Task Generic_GivenInsertedSnapshot_WhenReadInVariousReadModes_ThenReturnSameSnapshot<TSnapshot>(
        SnapshotAdder snapshotAdder)
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        var snapshotId = Id.NewId();

        var expectedSnapshot = TSnapshot.Construct(snapshotId).WithVersionNumber(new VersionNumber(5000));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.Write);

        await using var readOnlySnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.ReadOnly);

        await using var readOnlySecondaryPreferredSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.ReadOnlySecondaryPreferred);

        var inserted = await writeSnapshotRepository.PutSnapshot(snapshotId, expectedSnapshot);

        // ARRANGE ASSERTIONS

        inserted.ShouldBeTrue();

        // ACT

        var readOnlySnapshot = await readOnlySnapshotRepository.GetSnapshotOrDefault(snapshotId);

        var readOnlySecondaryPreferredSnapshot =
            await readOnlySecondaryPreferredSnapshotRepository.GetSnapshotOrDefault(snapshotId);

        // ASSERT

        readOnlySnapshot.ShouldBeEquivalentTo(expectedSnapshot);
        readOnlySecondaryPreferredSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task GivenInsertedSnapshot_WhenReadInVariousReadModes_ThenReturnSameSnapshot(SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }
}