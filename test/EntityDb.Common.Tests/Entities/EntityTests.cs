using System.Diagnostics.CodeAnalysis;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Tests.Implementations.Deltas;
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

    private static async Task<Source> BuildSource<TEntity>
    (
        IServiceScope serviceScope,
        Id entityId,
        Version from,
        Version to,
        TEntity? entity = default
    )
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        if (entity is not null) sourceBuilder.Load(entity);

        for (var i = from; i.Value <= to.Value; i = i.Next())
        {
            if (sourceBuilder.IsEntityKnown() &&
                sourceBuilder.GetEntity().Pointer.Version.Value >= i.Value) continue;

            sourceBuilder.Append(new DoNothing());
        }

        return sourceBuilder.Build(Id.NewId());
    }


    private async Task Generic_GivenEntityWithNVersions_WhenGettingAtVersionM_ThenReturnAtVersionM<TEntity>(
        SourcesAdder sourcesAdder, SnapshotAdder entitySnapshotAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        const ulong n = 10UL;
        const ulong m = 5UL;

        var versionN = new Version(n);

        var versionM = new Version(m);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entitySnapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.Write,
                TestSessionOptions.Write);

        var source = await BuildSource<TEntity>(serviceScope, entityId, new Version(1), versionN);

        var sourceCommitted = await entityRepository.Commit(source);

        // ARRANGE ASSERTIONS

        sourceCommitted.ShouldBeTrue();

        // ACT

        var entityAtVersionM = await entityRepository.GetSnapshot(entityId + versionM);

        // ASSERT

        entityAtVersionM.Pointer.Version.ShouldBe(versionM);
    }

    private async Task Generic_GivenExistingEntityWithNoSnapshot_WhenGettingEntity_ThenGetDeltasRuns<TEntity>(
        EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        var expectedVersion = new Version(10);

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

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.Write);

        var source =
            await BuildSource<TEntity>(serviceScope, entityId, new Version(1), expectedVersion);

        var sourceCommitted = await entityRepository.Commit(source);

        // ARRANGE ASSERTIONS

        sourceCommitted.ShouldBeTrue();

        // ACT

        var currenEntity = await entityRepository.GetSnapshot(entityId);

        // ASSERT

        currenEntity.Pointer.Version.ShouldBe(expectedVersion);

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

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository("NOT NULL", "NOT NULL");

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

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository("NOT NULL");

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

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository("NOT NULL", "NOT NULL");

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

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository("NOT NULL", "NOT NULL");

        var snapshotOrDefault = await entityRepository.GetSnapshot(default);

        // ASSERT

        snapshotOrDefault.ShouldNotBe(default);
        snapshotOrDefault.ShouldNotBe(snapshot);
        snapshotOrDefault.Pointer.Version.ShouldBe(
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

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(default!);

        // ASSERT

        await Should.ThrowAsync<SnapshotPointerDoesNotExistException>(async () =>
        {
            await entityRepository.GetSnapshot(default);
        });
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
}