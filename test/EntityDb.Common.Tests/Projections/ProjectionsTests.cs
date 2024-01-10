using System.Diagnostics.CodeAnalysis;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Shouldly;
using Xunit;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Projections;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ProjectionsTests : TestsBase<Startup>
{
    public ProjectionsTests(IServiceProvider startupServiceProvider, DatabaseContainerFixture databaseContainerFixture)
        : base(startupServiceProvider, databaseContainerFixture)
    {
    }

    private async Task Generic_GivenEmptySourceRepository_WhenGettingProjection_ThenThrow<TProjection>(
        SourcesAdder sourcesAdder, SnapshotAdder entitySnapshotAdder, SnapshotAdder projectionSnapshotAdder)
        where TProjection : IProjection<TProjection>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entitySnapshotAdder.AddDependencies.Invoke(serviceCollection);
            projectionSnapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var projectionRepository = await GetReadOnlyProjectionRepository<TProjection>(serviceScope, true);

        // ACT & ASSERT

        await Should.ThrowAsync<SnapshotPointerDoesNotExistException>(() =>
            projectionRepository.GetSnapshot(default));
    }

    private async Task
        Generic_GivenSourceCommitted_WhenGettingProjection_ThenReturnExpectedProjection<TEntity, TProjection>(
            SourcesAdder sourcesAdder, SnapshotAdder entitySnapshotAdder,
            SnapshotAdder projectionSnapshotAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
        where TProjection : class, IProjection<TProjection>, ISnapshotWithTestLogic<TProjection>
    {
        // ARRANGE

        const uint numberOfVersions = 5;
        const uint replaceAtVersionValue = 3;

        TProjection.ShouldRecordAsLatestLogic.Value = (projection, _) =>
            projection.Pointer.Version == new Version(replaceAtVersionValue);

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entitySnapshotAdder.AddDependencies.Invoke(serviceCollection);
            projectionSnapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);
        await using var projectionRepository = await GetReadOnlyProjectionRepository<TProjection>(serviceScope, true);

        entityRepository.Create(entityId);
        
        entityRepository.Seed(entityId, replaceAtVersionValue);
        
        var firstSourceCommitted = await entityRepository.Commit(Id.NewId());
        
        entityRepository.Seed(entityId, numberOfVersions - replaceAtVersionValue);

        var secondSourceCommitted = await entityRepository.Commit(Id.NewId());
        
        // ARRANGE ASSERTIONS

        numberOfVersions.ShouldBeGreaterThan(replaceAtVersionValue);

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeTrue();

        projectionRepository.SnapshotRepository.ShouldNotBeNull();

        // ACT

        var currentProjection = await projectionRepository.GetSnapshot(entityId);
        var projectionSnapshot = await projectionRepository.SnapshotRepository.GetSnapshotOrDefault(entityId);

        // ASSERT

        currentProjection.Pointer.Version.Value.ShouldBe(numberOfVersions);
        projectionSnapshot.ShouldNotBeNull();
        projectionSnapshot.Pointer.Version.Value.ShouldBe(replaceAtVersionValue);
    }

    [Theory]
    [MemberData(nameof(AddSourcesEntitySnapshotsAndProjectionSnapshots))]
    public Task GivenEmptySourceRepository_WhenGettingProjection_ThenThrow(SourcesAdder sourcesAdder,
        SnapshotAdder entitySnapshotAdder, SnapshotAdder projectionSnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { projectionSnapshotAdder.SnapshotType },
            new object?[] { sourcesAdder, entitySnapshotAdder, projectionSnapshotAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesEntitySnapshotsAndProjectionSnapshots))]
    public Task GivenSourceCommitted_WhenGettingProjection_ThenReturnExpectedProjection(
        SourcesAdder sourcesAdder, SnapshotAdder entitySnapshotAdder, SnapshotAdder projectionSnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entitySnapshotAdder.SnapshotType, projectionSnapshotAdder.SnapshotType },
            new object?[] { sourcesAdder, entitySnapshotAdder, projectionSnapshotAdder }
        );
    }
}