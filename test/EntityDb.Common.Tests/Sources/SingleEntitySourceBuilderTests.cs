using System.Diagnostics.CodeAnalysis;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Entities.Deltas;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Attributes;
using EntityDb.Common.Tests.Implementations.Deltas;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Sources;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class SingleEntitySourceBuilderTests : TestsBase<Startup>
{
    public SingleEntitySourceBuilderTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    private async Task Generic_GivenEntityNotKnown_WhenGettingEntity_ThenThrow<TEntity>(EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        // ASSERT

        sourceBuilder.IsEntityKnown().ShouldBeFalse();

        Should.Throw<KeyNotFoundException>(() => sourceBuilder.GetEntity());
    }

    private async Task Generic_GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity<TEntity>(
        EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedEntityId = Id.NewId();

        var expectedEntity = TEntity
            .Construct(expectedEntityId);

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, expectedEntityId);

        sourceBuilder.Load(expectedEntity);

        // ARRANGE ASSERTIONS

        sourceBuilder.IsEntityKnown().ShouldBeTrue();

        // ACT

        var actualEntityId = sourceBuilder.EntityId;
        var actualEntity = sourceBuilder.GetEntity();

        // ASSERT

        actualEntityId.ShouldBe(expectedEntityId);
        actualEntity.ShouldBe(expectedEntity);
    }

    private async Task
        Generic_GivenAddLeasesDelta_WhenBuildingNewEntityWithLease_ThenSourceDoesAddLeases<TEntity>(
            EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        // ACT

        var source = sourceBuilder
            .Append(new AddLease(new Lease(default!, default!, default!)))
            .Build(default);

        // ASSERT

        source.Messages.Length.ShouldBe(1);
        source.Messages[0].AddLeases.Length.ShouldBe(1);
    }

    private async Task Generic_GivenExistingEntityId_WhenUsingEntityIdForLoadTwice_ThenLoadThrows<TEntity>(
        EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddScoped(_ =>
                GetMockedSourceRepositoryFactory(
                    new object[] { new DoNothing() }));
        });

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(default!);

        var entity = await entityRepository.GetSnapshot(entityId);

        // ACT

        sourceBuilder.Load(entity);

        // ASSERT

        Should.Throw<EntityAlreadyLoadedException>(() => { sourceBuilder.Load(entity); });
    }

    private async Task
        Generic_GivenNonExistingEntityId_WhenAppendingDeltas_ThenVersionAutoIncrements<TEntity>(
            EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var numberOfVersionsToTest = new Version(10);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddScoped(_ =>
                GetMockedSourceRepositoryFactory());
        });

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        // ACT

        for (var i = new Version(1); i.Value <= numberOfVersionsToTest.Value; i = i.Next())
            sourceBuilder.Append(new DoNothing());

        var source = sourceBuilder.Build(default);

        // ASSERT

        for (var v = new Version(1); v.Value <= numberOfVersionsToTest.Value; v = v.Next())
        {
            var index = (int)(v.Value - 1);

            source.Messages[index].EntityPointer.Version.ShouldBe(v);
        }
    }

    private async Task Generic_GivenExistingEntity_WhenAppendingNewDelta_ThenSourceBuilds<TEntity>(
        EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddScoped(_ =>
                GetMockedSourceRepositoryFactory(new object[] { new DoNothing() }));
        });

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(default!);

        var entity = await entityRepository.GetSnapshot(entityId);

        // ACT

        var source = sourceBuilder
            .Load(entity)
            .Append(new DoNothing())
            .Build(default);

        // ASSERT

        source.Messages.Length.ShouldBe(1);

        source.Messages[0].Delta.ShouldBeEquivalentTo(new DoNothing());
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenEntityNotKnown_WhenGettingEntity_ThenThrow(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenAddLeasesDelta_WhenBuildingNewEntityWithLease_ThenSourceDoesAddLeases(
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenExistingEntityId_WhenUsingEntityIdForLoadTwice_ThenLoadThrows(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenNonExistingEntityId_WhenAppendingDeltas_ThenVersionAutoIncrements(
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenExistingEntity_WhenAppendingNewDelta_ThenSourceBuilds(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }
}