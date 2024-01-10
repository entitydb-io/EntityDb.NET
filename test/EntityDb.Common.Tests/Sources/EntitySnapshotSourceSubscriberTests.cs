using System.Diagnostics.CodeAnalysis;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Sources;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class EntitySnapshotSourceSubscriberTests : TestsBase<Startup>
{
    public EntitySnapshotSourceSubscriberTests(IServiceProvider startupServiceProvider,
        DatabaseContainerFixture databaseContainerFixture) : base(startupServiceProvider, databaseContainerFixture)
    {
    }

    private async Task
        Generic_GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsTrue_WhenRunningEntitySnapshotSourceSubscriber_ThenAlwaysWriteSnapshot<
            TEntity>(
            SourcesAdder sourcesAdder, SnapshotAdder snapshotAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        TEntity.ShouldRecordAsLatestLogic.Value = (_, _) => true;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            snapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        const uint numberOfVersions = 10;

        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);

        await using var snapshotRepository = await GetReadOnlySnapshotRepository<TEntity>(serviceScope);

        // ACT
        
        entityRepository.Create(entityId);
        entityRepository.Seed(entityId, numberOfVersions);
        
        var committed = await entityRepository
            .Commit(default);

        var snapshot = await snapshotRepository
            .GetSnapshotOrDefault(entityId);

        // ASSERT

        committed.ShouldBeTrue();
        snapshot.ShouldNotBeNull();
        snapshot.Pointer.Version.Value.ShouldBe(numberOfVersions);
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntitySnapshots))]
    private Task
        GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsTrue_WhenRunningEntitySnapshotSourceSubscriber_ThenAlwaysWriteSnapshot(
            SourcesAdder sourcesAdder, SnapshotAdder entitySnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entitySnapshotAdder.SnapshotType },
            new object?[] { sourcesAdder, entitySnapshotAdder }
        );
    }

    private async Task
        Generic_GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsFalse_WhenRunningEntitySnapshotSourceSubscriber_ThenNeverWriteSnapshot<
            TEntity>(SourcesAdder sourcesAdder, SnapshotAdder snapshotAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        TEntity.ShouldRecordAsLatestLogic.Value = (_, _) => false;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            snapshotAdder.AddDependencies.Invoke(serviceCollection);
        });
        
        await using var entityRepository = await GetWriteEntityRepository<TEntity>(serviceScope, true);
        await using var snapshotRepository = await GetReadOnlySnapshotRepository<TEntity>(serviceScope);

        var entityId = Id.NewId();

        // ACT

        entityRepository.Create(entityId);
        entityRepository.Seed(entityId, 10);
        
        await entityRepository.Commit(default);

        var snapshot = await snapshotRepository.GetSnapshotOrDefault(entityId);

        // ASSERT

        snapshot.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntitySnapshots))]
    private Task
        GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsFalse_WhenRunningEntitySnapshotSourceSubscriber_ThenNeverWriteSnapshot(
            SourcesAdder sourcesAdder, SnapshotAdder entitySnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entitySnapshotAdder.SnapshotType },
            new object?[] { sourcesAdder, entitySnapshotAdder }
        );
    }
}