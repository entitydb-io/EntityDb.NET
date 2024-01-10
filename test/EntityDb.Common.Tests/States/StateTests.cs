using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests.Implementations.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Shouldly;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.States;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class StateTests : TestsBase<Startup>
{
    public StateTests(IServiceProvider startupServiceProvider, DatabaseContainerFixture databaseContainerFixture) :
        base(startupServiceProvider, databaseContainerFixture)
    {
    }

    private async Task
        Generic_GivenEmptyStateRepository_WhenSnapshotPersistedAndFetched_ThenPersistedMatchesFetched<TState>(
            StateRepositoryAdder stateRepositoryAdder)
        where TState : class, IState<TState>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            stateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteStateRepository<TState>(serviceScope);

        var stateId = Id.NewId();
        var expectedState = TState.Construct(stateId + new Version(300));

        // ACT

        var persisted = await writeRepository.Put(stateId, expectedState);

        var actualState = await writeRepository.Get(stateId);

        // ASSERT

        persisted.ShouldBeTrue();

        actualState.ShouldBeEquivalentTo(expectedState);
    }

    [Theory]
    [MemberData(nameof(With_EntityState))]
    [MemberData(nameof(With_ProjectionState))]
    public Task GivenEmptyStateRepository_WhenSnapshotPersistedAndFetched_ThenPersistedMatchesFetched(
        StateRepositoryAdder stateRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { stateRepositoryAdder.StateType },
            new object?[] { stateRepositoryAdder }
        );
    }

    private async Task
        Generic_GivenEmptyStateRepository_WhenPersistingStateInReadOnlyMode_ThenReadOnlyWriteExceptionIsLogged<
            TState>(StateRepositoryAdder stateRepositoryAdder)
        where TState : class, IState<TState>
    {
        // ARRANGE

        var logs = new List<Log>();

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            stateRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(GetMockedLoggerFactory(logs));
        });

        await using var readOnlyRepository = await GetReadOnlyStateRepository<TState>(serviceScope);

        var stateSnapshot = TState.Construct(Id.NewId() + new Version(300));

        // ACT

        var persisted = await readOnlyRepository.Put(default, stateSnapshot);

        // ASSERT

        persisted.ShouldBeFalse();

        logs.Count(log => log.Exception is ReadOnlyWriteException).ShouldBe(1);
    }

    [Theory]
    [MemberData(nameof(With_EntityState))]
    [MemberData(nameof(With_ProjectionState))]
    public Task
        GivenEmptyStateRepository_WhenPersistingStateInReadOnlyMode_ThenReadOnlyWriteExceptionIsLogged(
            StateRepositoryAdder stateRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { stateRepositoryAdder.StateType },
            new object?[] { stateRepositoryAdder }
        );
    }

    private async Task
        Generic_GivenPersistedStateAsLatest_WhenStateDeleted_ThenReturnNoStates<TState>(
            StateRepositoryAdder stateRepositoryAdder)
        where TState : class, IStateWithTestLogic<TState>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            stateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteStateRepository<TState>(serviceScope);

        TState.ShouldRecordAsLatestLogic.Value = (_, _) => true;

        Pointer latestPointer = Id.NewId();

        var stateSnapshot = TState.Construct(latestPointer.Id + new Version(5000));

        var persisted = await writeRepository.Put(latestPointer, stateSnapshot);

        // ARRANGE ASSERTIONS

        persisted.ShouldBeTrue();

        // ACT

        var deleted = await writeRepository.Delete(new[] { latestPointer });

        var finalSnapshot = await writeRepository.Get(latestPointer);

        // ASSERT

        deleted.ShouldBeTrue();

        finalSnapshot.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(With_EntityState))]
    [MemberData(nameof(With_ProjectionState))]
    public Task GivenPersistedStateAsLatest_WhenStateDeleted_ThenReturnNoStates(
        StateRepositoryAdder stateRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { stateRepositoryAdder.StateType },
            new object?[] { stateRepositoryAdder }
        );
    }

    private async Task Generic_GivenPersistedState_WhenReadInVariousReadModes_ThenReturnSameState<TState>(
        StateRepositoryAdder stateRepositoryAdder)
        where TState : class, IState<TState>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            stateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteStateRepository<TState>(serviceScope);
        await using var readOnlyRepository = await GetReadOnlyStateRepository<TState>(serviceScope);
        await using var readOnlySecondaryRepository = await GetReadOnlyStateRepository<TState>(serviceScope, true);

        var stateId = Id.NewId();

        var expectedSnapshot = TState.Construct(stateId + new Version(5000));

        var persisted = await writeRepository.Put(stateId, expectedSnapshot);

        // ARRANGE ASSERTIONS

        persisted.ShouldBeTrue();

        // ACT

        var readOnlySnapshot = await readOnlyRepository.Get(stateId);

        var readOnlySecondaryPreferredSnapshot =
            await readOnlySecondaryRepository.Get(stateId);

        // ASSERT

        readOnlySnapshot.ShouldBeEquivalentTo(expectedSnapshot);
        readOnlySecondaryPreferredSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
    }

    [Theory]
    [MemberData(nameof(With_EntityState))]
    [MemberData(nameof(With_ProjectionState))]
    public Task GivenPersistedState_WhenReadInVariousReadModes_ThenReturnSameState(
        StateRepositoryAdder stateRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { stateRepositoryAdder.StateType },
            new object?[] { stateRepositoryAdder }
        );
    }
}
