using System.Diagnostics.CodeAnalysis;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Snapshots;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class SnapshotTests : TestsBase<Startup>
{
    public SnapshotTests(IServiceProvider startupServiceProvider, DatabaseContainerFixture databaseContainerFixture) :
        base(startupServiceProvider, databaseContainerFixture)
    {
    }

    private async Task
        Generic_GivenEmptySnapshotRepository_WhenSnapshotCommittedAndFetched_ThenCommittedMatchesFetched<TSnapshot>(
            SnapshotAdder snapshotAdder)
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();
        var expectedSnapshot = TSnapshot.Construct(entityId).WithVersion(new Version(300));

        var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>();

        await using var snapshotRepository = await snapshotRepositoryFactory
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var snapshotCommitted = await snapshotRepository.PutSnapshot(entityId, expectedSnapshot);

        var actualSnapshot = await snapshotRepository.GetSnapshotOrDefault(entityId);

        // ASSERT

        snapshotCommitted.ShouldBeTrue();

        actualSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task GivenEmptySnapshotRepository_WhenSnapshotCommittedAndFetched_ThenCommittedMatchesFetched(
        SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }

    private async Task
        Generic_GivenEmptySnapshotRepository_WhenCommittingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged<
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

        var snapshot = TSnapshot.Construct(default).WithVersion(new Version(300));

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
        GivenEmptySnapshotRepository_WhenCommittingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged(
            SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }

    private async Task
        Generic_GivenCommittedSnapshotAsLatest_WhenSnapshotDeleted_ThenReturnNoSnapshot<TSnapshot>(
            SnapshotAdder snapshotAdder)
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        Pointer latestPointer = Id.NewId();

        var snapshot = TSnapshot.Construct(latestPointer).WithVersion(new Version(5000));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotAdder.AddDependencies.Invoke(serviceCollection);

            TSnapshot.ShouldRecordAsLatestLogic.Value = (_, _) => true;
        });

        await using var writeSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.Write);

        var inserted = await writeSnapshotRepository.PutSnapshot(latestPointer, snapshot);

        // ARRANGE ASSERTIONS

        inserted.ShouldBeTrue();

        // ACT

        var deleted = await writeSnapshotRepository.DeleteSnapshots(new[] { latestPointer });

        var finalSnapshot = await writeSnapshotRepository.GetSnapshotOrDefault(latestPointer);

        // ASSERT

        deleted.ShouldBeTrue();

        finalSnapshot.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task GivenCommittedSnapshotAsLatest_WhenSnapshotDeleted_ThenReturnNoSnapshot(SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }

    private async Task Generic_GivenCommittedSnapshot_WhenReadInVariousReadModes_ThenReturnSameSnapshot<TSnapshot>(
        SnapshotAdder snapshotAdder)
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        var entityId = Id.NewId();

        var expectedSnapshot = TSnapshot.Construct(entityId).WithVersion(new Version(5000));

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

        var inserted = await writeSnapshotRepository.PutSnapshot(entityId, expectedSnapshot);

        // ARRANGE ASSERTIONS

        inserted.ShouldBeTrue();

        // ACT

        var readOnlySnapshot = await readOnlySnapshotRepository.GetSnapshotOrDefault(entityId);

        var readOnlySecondaryPreferredSnapshot =
            await readOnlySecondaryPreferredSnapshotRepository.GetSnapshotOrDefault(entityId);

        // ASSERT

        readOnlySnapshot.ShouldBeEquivalentTo(expectedSnapshot);
        readOnlySecondaryPreferredSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task GivenCommittedSnapshot_WhenReadInVariousReadModes_ThenReturnSameSnapshot(SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }
}