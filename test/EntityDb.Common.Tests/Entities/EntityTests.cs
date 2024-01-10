using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Sources.Agents;
using EntityDb.Common.Tests.Implementations.Entities.Deltas;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.States;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Entities;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class EntityTests : TestsBase<Startup>
{
    public EntityTests(IServiceProvider serviceProvider, DatabaseContainerFixture databaseContainerFixture) : base(
        serviceProvider, databaseContainerFixture)
    {
    }

    private async Task Generic_GivenEntityWithNVersions_WhenGettingAtVersionM_ThenReturnAtVersionM<TEntity>(
        SourceRepositoryAdder sourceRepositoryAdder, StateRepositoryAdder entityStateRepositoryAdder)
        where TEntity : class, IEntity<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            entityStateRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);
        await using var readOnlyRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, true);

        const ulong n = 10UL;
        const ulong m = 5UL;

        var versionM = new Version(m);

        var entityId = Id.NewId();

        writeRepository.Create(entityId);
        writeRepository.Seed(entityId, m);
        writeRepository.Seed(entityId, n - m);

        var committed = await writeRepository.Commit();

        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();

        // ACT

        await readOnlyRepository.Load(entityId + new Version(m));

        var entity = readOnlyRepository.Get(entityId);

        // ASSERT

        entity.GetPointer().Version.ShouldBe(versionM);
    }

    private async Task Generic_GivenExistingEntityWithNoPersistedState_WhenGettingEntity_ThenGetDeltasRuns<TEntity>(
        EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : class, IEntity<TEntity>
    {
        // ARRANGE

        const uint expectedVersionNumber = 10;
        var expectedVersion = new Version(expectedVersionNumber);

        var entityId = Id.NewId();

        var deltas = new List<object>();

        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        sourceRepositoryMock
            .Setup(repository =>
                repository.Commit(It.IsAny<Source>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Source source, CancellationToken _) =>
            {
                deltas.AddRange(source.Messages.Select(message => message.Delta));

                return true;
            });

        sourceRepositoryMock
            .Setup(factory => factory.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateDeltas(It.IsAny<IMessageDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Returns(() => AsyncEnumerablePolyfill.FromResult(deltas))
            .Verifiable();

        var sourceRepositoryFactoryMock =
            new Mock<ISourceRepositoryFactory>(MockBehavior.Strict);

        sourceRepositoryFactoryMock
            .Setup(factory => factory.Create(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceRepositoryMock.Object);

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(sourceRepositoryFactoryMock.Object);
        });

        await using var writeRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);
        await using var readOnlyRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, false);

        writeRepository.Create(entityId);
        writeRepository.Seed(entityId, expectedVersionNumber);

        var committed = await writeRepository.Commit();

        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();

        // ACT

        await readOnlyRepository.Load(entityId);

        var entity = readOnlyRepository.Get(entityId);

        // ASSERT

        entity.GetPointer().Version.ShouldBe(expectedVersion);

        sourceRepositoryMock
            .Verify(
                repository => repository.EnumerateDeltas(It.IsAny<IMessageDataDataQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
    }

    private async Task
        Generic_GivenNoStateRepositoryFactory_WhenCreatingEntityRepository_ThenNoStateRepository<TEntity>(
            EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : class, IEntity<TEntity>, IStateWithTestLogic<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory());
        });

        // ACT

        await using var stateRepositoryFactory = serviceScope.ServiceProvider
            .GetService<IStateRepositoryFactory<TEntity>>();

        await using var entityRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, true);

        // ASSERT

        stateRepositoryFactory.ShouldBeNull();

        entityRepository.StateRepository.ShouldBeNull();
    }

    private async Task
        Generic_GivenNoStateSessionOptions_WhenCreatingEntityRepository_ThenNoStateRepository<TEntity>(
            EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : class, IEntity<TEntity>, IStateWithTestLogic<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory());
            serviceCollection.AddSingleton(GetMockedStateRepositoryFactory<TEntity>());
        });

        // ACT

        await using var stateRepositoryFactory = serviceScope.ServiceProvider
            .GetService<IStateRepositoryFactory<TEntity>>();

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        // ASSERT

        stateRepositoryFactory.ShouldNotBeNull();

        entityRepository.StateRepository.ShouldBeNull();
    }

    private async Task
        Generic_GivenStateRepositoryFactoryAndStateSessionOptions_WhenCreatingEntityRepository_ThenNoStateRepository<
            TEntity>(EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : class, IEntity<TEntity>, IStateWithTestLogic<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory());
            serviceCollection.AddSingleton(GetMockedStateRepositoryFactory<TEntity>());
        });

        // ACT

        await using var stateRepositoryFactory = serviceScope.ServiceProvider
            .GetService<IStateRepositoryFactory<TEntity>>();

        await using var entityRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, true);

        // ASSERT

        stateRepositoryFactory.ShouldNotBeNull();

        entityRepository.StateRepository.ShouldNotBeNull();
    }

    private async Task
        Generic_GivenPersistedStateAndNewDeltas_WhenGettingCurrentState_ThenReturnNewerThanPersistedState<TEntity>(
            EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : class, IEntity<TEntity>
    {
        // ARRANGE

        var previousEntity = TEntity.Construct(default(Id) + new Version(1));

        var newDeltas = new object[] { new DoNothing(), new DoNothing() };

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(newDeltas));
            serviceCollection.AddSingleton(GetMockedStateRepositoryFactory(previousEntity));
        });

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);

        // ACT

        await entityRepository.Load(default);

        var currentEntity = entityRepository.Get(default);

        // ASSERT

        currentEntity.ShouldNotBe(previousEntity);
        currentEntity.GetPointer().Version.ShouldBe(
            new Version(previousEntity.GetPointer().Version.Value + Convert.ToUInt64(newDeltas.Length)));
    }

    private async Task Generic_GivenNonExistentEntityId_WhenGettingCurrentEntity_ThenThrow<TEntity>(
        EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : class, IEntity<TEntity>, IStateWithTestLogic<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory());
        });

        await using var entityRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, false);

        // ASSERT

        Should.Throw<UnknownEntityIdException>(() => entityRepository.Get(default));
    }

    private async Task Generic_GivenEntityCommitted_WhenGettingEntity_ThenReturnEntity<TEntity>(
        SourceRepositoryAdder sourceRepositoryAdder, EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : class, IEntity<TEntity>
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        var expectedEntity = TEntity.Construct(entityId + new Version(1));

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        var source = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[] { new Message { Id = Id.NewId(), StatePointer = entityId, Delta = new DoNothing() } },
        };

        var committed = await entityRepository.SourceRepository.Commit(source);

        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();

        // ACT

        await entityRepository.Load(entityId);

        var actualEntity = entityRepository.Get(entityId);

        // ASSERT

        actualEntity.ShouldBeEquivalentTo(expectedEntity);
    }

    [Theory]
    [MemberData(nameof(With_Source_EntityState))]
    public Task GivenEntityWithNVersions_WhenGettingAtVersionM_ThenReturnAtVersionM(
        SourceRepositoryAdder sourceRepositoryAdder,
        StateRepositoryAdder entityStateRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityStateRepositoryAdder.StateType },
            new object?[] { sourceRepositoryAdder, entityStateRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Entity))]
    public Task GivenExistingEntityWithNoPersistedState_WhenGettingEntity_ThenGetDeltasRuns(
        EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Entity))]
    public Task GivenNoStateRepositoryFactory_WhenCreatingEntityRepository_ThenNoStateRepository(
        EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Entity))]
    public Task GivenNoStateSessionOptions_WhenCreatingEntityRepository_ThenNoStateRepository(
        EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Entity))]
    public Task
        GivenStateRepositoryFactoryAndStateSessionOptions_WhenCreatingEntityRepository_ThenNoStateRepository(
            EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Entity))]
    public Task GivenPersistedStateAndNewDeltas_WhenGettingCurrentState_ThenReturnNewerThanPersistedState(
        EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Entity))]
    public Task GivenNonExistentEntityId_WhenGettingCurrentEntity_ThenThrow(EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { entityRepositoryAdder }
        );
    }

    [Theory]
    [MemberData(nameof(With_Source_Entity))]
    public Task GivenEntityCommitted_WhenGettingEntity_ThenReturnEntity(SourceRepositoryAdder sourceRepositoryAdder,
        EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { sourceRepositoryAdder, entityRepositoryAdder }
        );
    }
}
