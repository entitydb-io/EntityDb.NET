using System.Diagnostics.CodeAnalysis;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Attributes;
using EntityDb.Common.Tests.Implementations.Deltas;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Entities;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class EntityRepositoryTests : TestsBase<Startup>
{
    public EntityRepositoryTests(IServiceProvider serviceProvider, DatabaseContainerFixture databaseContainerFixture) : base(
        serviceProvider, databaseContainerFixture)
    {
    }

    private async Task Generic_GivenEntityNotKnown_WhenGettingEntity_ThenThrow<TEntity>(SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });     
        
        var entityRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, false);
        
        // ASSERT

        Should.Throw<EntityNotLoadedException>(() => entityRepository.Get(default));
    }

    private async Task Generic_GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity<TEntity>(SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
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

    private async Task Generic_GivenAddLeasesDelta_WhenBuildingNewEntityWithLease_ThenSourceDoesAddLeases<TEntity>(SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var committedSources = new List<Source>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceSubscriber(committedSources));

            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        // ACT

        entityRepository.Create(default);
        
        entityRepository.Append(default, new AddLease(new Lease(default!, default!, default!)));

        await entityRepository.Commit(default);

        // ASSERT

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].AddLeases.Length.ShouldBe(1);
    }

    private async Task Generic_GivenExistingEntityId_WhenUsingEntityIdForLoadTwice_ThenLoadThrows<TEntity>(SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        writeRepository.Create(entityId);
        writeRepository.Append(entityId, new DoNothing());

        var committed = await writeRepository.Commit(default);
        
        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();
        
        // ACT

        await using var readRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, false);

        await readRepository.Load(entityId);

        // ASSERT

        await Should.ThrowAsync<EntityAlreadyLoadedException>(readRepository.Load(entityId));
    }

    private async Task Generic_GivenNonExistingEntityId_WhenAppendingDeltas_ThenVersionAutoIncrements<TEntity>(SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var committedSources = new List<Source>();

        var numberOfVersionsToTest = 10;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceSubscriber(committedSources));
            
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        entityRepository.Create(default);
        
        // ACT

        for (var i = 1; i <= numberOfVersionsToTest; i++)
        {
            entityRepository.Append(default, new DoNothing());
        }

        await entityRepository.Commit(default);

        // ASSERT

        committedSources.Count.ShouldBe(1);

        var expectedVersion = Version.Zero;
        
        for (var i = 1; i <= numberOfVersionsToTest; i++)
        {
            expectedVersion = expectedVersion.Next();
            
            committedSources[0].Messages[i - 1].EntityPointer.Version.ShouldBe(expectedVersion);
        }
    }

    private async Task Generic_GivenExistingEntity_WhenAppendingNewDelta_ThenSourceBuilds<TEntity>(SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var committedSources = new List<Source>();
        
        var expectedDelta = new DoNothing();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceSubscriber(committedSources));
            
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        // ACT

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        entityRepository.Create(default);
        entityRepository.Append(default, expectedDelta);

        await entityRepository.Commit(default);

        // ASSERT

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].Delta.ShouldBeEquivalentTo(new DoNothing());
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenEntityNotKnown_WhenGettingEntity_ThenThrow(SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity(SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenAddLeasesDelta_WhenBuildingNewEntityWithLease_ThenSourceDoesAddLeases(SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenExistingEntityId_WhenUsingEntityIdForLoadTwice_ThenLoadThrows(SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenNonExistingEntityId_WhenAppendingDeltas_ThenVersionAutoIncrements(SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenExistingEntity_WhenAppendingNewDelta_ThenSourceBuilds(SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }
}