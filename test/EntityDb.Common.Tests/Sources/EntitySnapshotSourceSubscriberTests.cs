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
            SourcesAdder sourcesAdder, SnapshotAdder entitySnapshotAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        TEntity.ShouldRecordAsLatestLogic.Value = (_, _) => true;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entitySnapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        const uint numberOfVersions = 10;

        var source = SourceSeeder.Create<TEntity>(entityId, numberOfVersions);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.Write);

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnly);

        // ACT

        var committed = await entityRepository.Commit(source);

        var snapshot = await snapshotRepository.GetSnapshotOrDefault(entityId);

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

        var entityId = Id.NewId();

        var source = SourceSeeder.Create<TEntity>(entityId, 10);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.Write);

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnly);

        // ACT

        await entityRepository.Commit(source);

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