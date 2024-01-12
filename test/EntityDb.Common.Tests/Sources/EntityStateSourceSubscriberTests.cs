using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.States;
using Shouldly;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EntityDb.Common.Tests.Sources;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class EntityStateSourceSubscriberTests : TestsBase<Startup>
{
    public EntityStateSourceSubscriberTests(IServiceProvider startupServiceProvider,
        DatabaseContainerFixture databaseContainerFixture) : base(startupServiceProvider, databaseContainerFixture)
    {
    }

    private async Task
        Generic_GivenStateShouldRecordAsMostRecentAlwaysReturnsTrue_WhenRunningEntityStateSourceSubscriber_ThenAlwaysPersistState<
            TEntity>(
            SourceRepositoryAdder sourceRepositoryAdder, StateRepositoryAdder stateRepositoryAdder)
        where TEntity : class, IEntity<TEntity>, IStateWithTestLogic<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            stateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);

        await using var stateRepository = await GetReadOnlyStateRepository<TEntity>(serviceScope);

        TEntity.ShouldRecordAsLatestLogic.Value = (_, _) => true;

        var entityId = Id.NewId();

        const uint numberOfVersions = 10;

        // ACT

        entityRepository.Create(entityId);
        entityRepository.Seed(entityId, numberOfVersions);

        var committed = await entityRepository.Commit();

        var persistedEntity = await stateRepository.Get(entityId);

        // ASSERT

        committed.ShouldBeTrue();
        persistedEntity.ShouldNotBeNull();
        persistedEntity.GetPointer().Version.Value.ShouldBe(numberOfVersions);
    }

    [Theory]
    [MemberData(nameof(With_Source_EntityState))]
    private Task
        GivenStateShouldRecordAsMostRecentAlwaysReturnsTrue_WhenRunningEntityStateSourceSubscriber_ThenAlwaysPersistState(
            SourceRepositoryAdder sourceRepositoryAdder, StateRepositoryAdder entityStateRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityStateRepositoryAdder.StateType },
            new object?[] { sourceRepositoryAdder, entityStateRepositoryAdder }
        );
    }

    private async Task
        Generic_GivenStateShouldRecordAsMostRecentAlwaysReturnsFalse_WhenRunningEntityStateSourceSubscriber_ThenNeverPersistState<
            TEntity>(SourceRepositoryAdder sourceRepositoryAdder, StateRepositoryAdder stateRepositoryAdder)
        where TEntity : class, IEntity<TEntity>, IStateWithTestLogic<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            stateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);
        await using var stateRepository = await GetReadOnlyStateRepository<TEntity>(serviceScope);

        TEntity.ShouldRecordAsLatestLogic.Value = (_, _) => false;

        var entityId = Id.NewId();

        // ACT

        entityRepository.Create(entityId);
        entityRepository.Seed(entityId, 10);

        await entityRepository.Commit();

        var persistedEntity = await stateRepository.Get(entityId);

        // ASSERT

        persistedEntity.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(With_Source_EntityState))]
    private Task
        GivenStateShouldRecordAsMostRecentAlwaysReturnsFalse_WhenRunningEntityStateSourceSubscriber_ThenNeverPersistState(
            SourceRepositoryAdder sourceRepositoryAdder, StateRepositoryAdder entityStateRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityStateRepositoryAdder.StateType },
            new object?[] { sourceRepositoryAdder, entityStateRepositoryAdder }
        );
    }
}
