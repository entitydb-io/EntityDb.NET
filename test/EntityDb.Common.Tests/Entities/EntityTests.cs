using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Sources.Agents;
using EntityDb.Common.Tests.Implementations.Deltas;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
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
        SourcesAdder sourcesAdder, SnapshotAdder entitySnapshotAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        const ulong n = 10UL;
        const ulong m = 5UL;

        var versionM = new Version(m);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entitySnapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        await using var writeRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);
        
        writeRepository.Create(entityId);
        writeRepository.Seed(entityId, m);
        writeRepository.Seed(entityId, n - m);
        
        var committed = await writeRepository.Commit(default);
        
        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();

        // ACT

        await using var readOnlyRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, true);

        await readOnlyRepository.Load(entityId + new Version(m));
        
        var entity = readOnlyRepository.Get(entityId);
        
        // ASSERT

        entity.Pointer.Version.ShouldBe(versionM);
    }

    private async Task Generic_GivenExistingEntityWithNoSnapshot_WhenGettingEntity_ThenGetDeltasRuns<TEntity>(
        EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
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
            .Setup(repository => repository.EnumerateDeltas(It.IsAny<IMessageQuery>(), It.IsAny<CancellationToken>()))
            .Returns(() => AsyncEnumerablePolyfill.FromResult(deltas))
            .Verifiable();

        var sourceRepositoryFactoryMock =
            new Mock<ISourceRepositoryFactory>(MockBehavior.Strict);

        sourceRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceRepositoryMock.Object);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(sourceRepositoryFactoryMock.Object);
        });

        await using var writeRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        writeRepository.Create(entityId);
        writeRepository.Seed(entityId, expectedVersionNumber);
        
        var committed = await writeRepository.Commit(default);

        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();

        // ACT

        await using var readOnlyRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, false);

        await readOnlyRepository.Load(entityId);
        
        var entity = readOnlyRepository.Get(entityId);

        // ASSERT

        entity.Pointer.Version.ShouldBe(expectedVersion);

        sourceRepositoryMock
            .Verify(
                repository => repository.EnumerateDeltas(It.IsAny<IMessageQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
    }

    private async Task
        Generic_GivenNoSnapshotRepositoryFactory_WhenCreatingEntityRepository_ThenNoSnapshotRepository<TEntity>(
            EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory());
        });

        // ACT

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetService<ISnapshotRepositoryFactory<TEntity>>();

        await using var entityRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, true);

        // ASSERT

        snapshotRepositoryFactory.ShouldBeNull();

        entityRepository.SnapshotRepository.ShouldBeNull();
    }

    private async Task
        Generic_GivenNoSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository<TEntity>(
            EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory());
            serviceCollection.AddSingleton(GetMockedSnapshotRepositoryFactory<TEntity>());
        });

        // ACT

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetService<ISnapshotRepositoryFactory<TEntity>>();

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        // ASSERT

        snapshotRepositoryFactory.ShouldNotBeNull();

        entityRepository.SnapshotRepository.ShouldBeNull();
    }

    private async Task
        Generic_GivenSnapshotRepositoryFactoryAndSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository<
            TEntity>(EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory());
            serviceCollection.AddSingleton(GetMockedSnapshotRepositoryFactory<TEntity>());
        });

        // ACT

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetService<ISnapshotRepositoryFactory<TEntity>>();

        await using var entityRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, true);

        // ASSERT

        snapshotRepositoryFactory.ShouldNotBeNull();

        entityRepository.SnapshotRepository.ShouldNotBeNull();
    }

    private async Task
        Generic_GivenSnapshotAndNewDeltas_WhenGettingSnapshotOrDefault_ThenReturnNewerThanSnapshot<TEntity>(
            EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        var snapshot = TEntity.Construct(default).WithVersion(new Version(1));

        var newDeltas = new object[]
        {
            new DoNothing(),
            new DoNothing(),
        };

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(newDeltas));
            serviceCollection.AddSingleton(GetMockedSnapshotRepositoryFactory(snapshot));
        });

        // ACT

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);

        await entityRepository.Load(default);
        
        var entity = entityRepository.Get(default);

        // ASSERT

        entity.ShouldNotBe(snapshot);
        entity.Pointer.Version.ShouldBe(
            new Version(snapshot.Pointer.Version.Value + Convert.ToUInt64(newDeltas.Length)));
    }

    private async Task Generic_GivenNonExistentEntityId_WhenGettingCurrentEntity_ThenThrow<TEntity>(
        EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory());
        });

        await using var entityRepository = await GetReadOnlyEntityRepository<TEntity>(serviceScope, false);

        // ASSERT

        Should.Throw<EntityNotLoadedException>(() => entityRepository.Get(default));
    }
    
    private async Task Generic_GivenEntityCommitted_WhenGettingEntity_ThenReturnEntity<TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE
    
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });
    
        var entityId = Id.NewId();
    
        var expectedEntity = TEntity.Construct(entityId).WithVersion(new Version(1));
    
        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, false);

        var source = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = Id.NewId(),
                    EntityPointer = entityId,
                    Delta = new DoNothing(),
                }
            }.ToImmutableArray()
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
    [MemberData(nameof(AddSourcesAndEntitySnapshots))]
    public Task GivenEntityWithNVersions_WhenGettingAtVersionM_ThenReturnAtVersionM(SourcesAdder sourcesAdder,
        SnapshotAdder entitySnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entitySnapshotAdder.SnapshotType },
            new object?[] { sourcesAdder, entitySnapshotAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenExistingEntityWithNoSnapshot_WhenGettingEntity_ThenGetDeltasRuns(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenNoSnapshotRepositoryFactory_WhenCreatingEntityRepository_ThenNoSnapshotRepository(
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
    public Task GivenNoSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository(
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
    public Task
        GivenSnapshotRepositoryFactoryAndSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository(
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
    public Task GivenSnapshotAndNewDeltas_WhenGettingSnapshotOrDefault_ThenReturnNewerThanSnapshot(
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
    public Task GivenNonExistentEntityId_WhenGettingCurrentEntity_ThenThrow(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenEntityCommitted_WhenGettingEntity_ThenReturnEntity(SourcesAdder sourcesAdder,
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }
}