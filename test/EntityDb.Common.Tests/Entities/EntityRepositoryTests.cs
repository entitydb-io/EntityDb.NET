using EntityDb.Abstractions;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.States;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Attributes;
using EntityDb.Common.Tests.Implementations.Entities.Deltas;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EntityDb.Common.Tests.Entities;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class EntityRepositoryTests : TestsBase<Startup>
{
    public EntityRepositoryTests(IServiceProvider serviceProvider, DatabaseContainerFixture databaseContainerFixture) :
        base(
            serviceProvider, databaseContainerFixture)
    {
    }

    private async Task Generic_GivenEntityNotKnown_WhenGettingEntity_ThenThrow<TEntity>(
        SourceRepositoryAdder sourceRepositoryAdder, EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, false);

        // ASSERT

        Should.Throw<UnknownEntityException>(() => entityRepository.Get(default));
    }

    private async Task Generic_GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity<TEntity>(
        SourceRepositoryAdder sourceRepositoryAdder, EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedEntityId = Id.NewId();

        var expectedEntity = TEntity
            .Construct(expectedEntityId);

        var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        entityRepository.Create(expectedEntityId);

        // ARRANGE ASSERTIONS

        Should.NotThrow(() => entityRepository.Get(expectedEntityId));

        // ACT

        var actualEntity = entityRepository.Get(expectedEntityId);
        var actualEntityId = actualEntity.GetPointer().Id;

        // ASSERT

        actualEntity.ShouldBeEquivalentTo(expectedEntity);
        actualEntityId.ShouldBe(expectedEntityId);
    }

    private async Task Generic_GivenAddLeasesDelta_WhenBuildingNewEntityWithLease_ThenSourceDoesAddLeases<TEntity>(
        SourceRepositoryAdder sourceRepositoryAdder, EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var committedSources = new List<Source>();

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceSubscriber(committedSources));

            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        // ACT

        writeRepository.Create(default);

        writeRepository.Append(default, new AddLease(new Lease(default!, default!, default!)));

        await writeRepository.Commit();

        // ASSERT

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].AddLeases.Length.ShouldBe(1);
    }

    private async Task Generic_GivenNonExistingEntityId_WhenAppendingDeltas_ThenVersionAutoIncrements<TEntity>(
        SourceRepositoryAdder sourceRepositoryAdder, EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var committedSources = new List<Source>();

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceSubscriber(committedSources));

            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        const int numberOfVersionsToTest = 10;

        writeRepository.Create(default);

        // ACT

        for (var i = 1; i <= numberOfVersionsToTest; i++)
        {
            writeRepository.Append(default, new DoNothing());
        }

        await writeRepository.Commit();

        // ASSERT

        committedSources.Count.ShouldBe(1);

        var expectedVersion = StateVersion.Zero;

        for (var i = 1; i <= numberOfVersionsToTest; i++)
        {
            expectedVersion = expectedVersion.Next();

            committedSources[0].Messages[i - 1].StatePointer.StateVersion.ShouldBe(expectedVersion);
        }
    }

    private async Task Generic_GivenExistingEntity_WhenAppendingNewDelta_ThenSourceBuilds<TEntity>(
        SourceRepositoryAdder sourceRepositoryAdder, EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var committedSources = new List<Source>();

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceSubscriber(committedSources));

            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        var expectedDelta = new DoNothing();

        // ACT

        entityRepository.Create(default);
        entityRepository.Append(default, expectedDelta);

        await entityRepository.Commit();

        // ASSERT

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].Delta.ShouldBeEquivalentTo(new DoNothing());
    }

    [Theory]
    [MemberData(nameof(With_Source_Entity))]
    public Task GivenEntityNotKnown_WhenGettingEntity_ThenThrow(SourceRepositoryAdder sourceRepositoryAdder,
        EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { sourceRepositoryAdder, entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Source_Entity))]
    public Task GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity(SourceRepositoryAdder sourceRepositoryAdder,
        EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { sourceRepositoryAdder, entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Source_Entity))]
    public Task GivenAddLeasesDelta_WhenBuildingNewEntityWithLease_ThenSourceDoesAddLeases(
        SourceRepositoryAdder sourceRepositoryAdder, EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { sourceRepositoryAdder, entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Source_Entity))]
    public Task GivenNonExistingEntityId_WhenAppendingDeltas_ThenVersionAutoIncrements(
        SourceRepositoryAdder sourceRepositoryAdder, EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { sourceRepositoryAdder, entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Source_Entity))]
    public Task GivenExistingEntity_WhenAppendingNewDelta_ThenSourceBuilds(SourceRepositoryAdder sourceRepositoryAdder,
        EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { sourceRepositoryAdder, entityRepositoryAdder }
        );
    }
}
