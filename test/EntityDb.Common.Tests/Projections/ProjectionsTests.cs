using EntityDb.Abstractions;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.States;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.States;
using Shouldly;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EntityDb.Common.Tests.Projections;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class ProjectionsTests : TestsBase<Startup>
{
    public ProjectionsTests(IServiceProvider startupServiceProvider, DatabaseContainerFixture databaseContainerFixture)
        : base(startupServiceProvider, databaseContainerFixture)
    {
    }

    private async Task Generic_GivenEmptySourceRepository_WhenGettingProjection_ThenThrow<TProjection>(
        SourceRepositoryAdder sourceRepositoryAdder, StateRepositoryAdder entityStateRepositoryAdder,
        StateRepositoryAdder projectionStateRepositoryAdder)
        where TProjection : IProjection<TProjection>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            entityStateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            projectionStateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var readOnlyRepository = await GetReadOnlyProjectionRepository<TProjection>(serviceScope, true);

        // ACT & ASSERT

        await Should.ThrowAsync<StateDoesNotExistException>(() =>
            readOnlyRepository.Get(default));
    }

    private async Task
        Generic_GivenSourceCommitted_WhenGettingProjection_ThenReturnExpectedProjection<TEntity, TProjection>(
            SourceRepositoryAdder sourceRepositoryAdder, StateRepositoryAdder entityStateRepositoryAdder,
            StateRepositoryAdder projectionStateRepositoryAdder)
        where TEntity : class, IEntity<TEntity>, IStateWithTestLogic<TEntity>
        where TProjection : class, IProjection<TProjection>, IStateWithTestLogic<TProjection>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            entityStateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            projectionStateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);
        await using var projectionRepository = await GetReadOnlyProjectionRepository<TProjection>(serviceScope, true);

        const uint numberOfVersions = 5;
        const uint replaceAtVersionValue = 3;

        TProjection.ShouldRecordAsLatestLogic.Value = (projection, _) =>
            projection.GetPointer().StateVersion == new StateVersion(replaceAtVersionValue);

        var stateId = Id.NewId();

        entityRepository.Create(stateId);

        entityRepository.Seed(stateId, replaceAtVersionValue);

        var firstSourceCommitted = await entityRepository.Commit();

        entityRepository.Seed(stateId, numberOfVersions - replaceAtVersionValue);

        var secondSourceCommitted = await entityRepository.Commit();

        // ARRANGE ASSERTIONS

        numberOfVersions.ShouldBeGreaterThan(replaceAtVersionValue);

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeTrue();

        projectionRepository.StateRepository.ShouldNotBeNull();

        // ACT

        var currentProjection = await projectionRepository.Get(stateId);
        var persistedProjection = await projectionRepository.StateRepository.Get(stateId);

        // ASSERT

        currentProjection.GetPointer().StateVersion.Value.ShouldBe(numberOfVersions);
        persistedProjection.ShouldNotBeNull();
        persistedProjection.GetPointer().StateVersion.Value.ShouldBe(replaceAtVersionValue);
    }

    [Theory]
    [MemberData(nameof(With_Source_EntityState_ProjectionState))]
    public Task GivenEmptySourceRepository_WhenGettingProjection_ThenThrow(SourceRepositoryAdder sourceRepositoryAdder,
        StateRepositoryAdder entityStateRepositoryAdder, StateRepositoryAdder projectionStateRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { projectionStateRepositoryAdder.StateType },
            new object?[] { sourceRepositoryAdder, entityStateRepositoryAdder, projectionStateRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Source_EntityState_ProjectionState))]
    public Task GivenSourceCommitted_WhenGettingProjection_ThenReturnExpectedProjection(
        SourceRepositoryAdder sourceRepositoryAdder, StateRepositoryAdder entityStateRepositoryAdder,
        StateRepositoryAdder projectionStateRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityStateRepositoryAdder.StateType, projectionStateRepositoryAdder.StateType },
            new object?[] { sourceRepositoryAdder, entityStateRepositoryAdder, projectionStateRepositoryAdder }
        );
    }
}
