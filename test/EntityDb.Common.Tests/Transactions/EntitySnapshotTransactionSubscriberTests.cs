using System;
using System.Threading.Tasks;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Transactions;

public class EntitySnapshotTransactionSubscriberTests : TestsBase<Startup>
{
    public EntitySnapshotTransactionSubscriberTests(IServiceProvider startupServiceProvider) : base(
        startupServiceProvider)
    {
    }


    private async Task
        Generic_GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsTrue_WhenRunningEntitySnapshotTransactionSubscriber_ThenAlwaysWriteSnapshot<
            TEntity>(
            TransactionsAdder transactionsAdder, SnapshotAdder entitySnapshotAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        TEntity.ShouldRecordAsLatestLogic.Value = (_, _) => true;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entitySnapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        const uint numberOfVersionNumbers = 10;

        var transaction = TransactionSeeder.Create<TEntity>(entityId, numberOfVersionNumbers);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.Write);

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnly);

        // ACT

        await entityRepository.PutTransaction(transaction);

        var snapshot = await snapshotRepository.GetSnapshotOrDefault(entityId);

        // ASSERT

        snapshot.ShouldNotBe(default);
        snapshot!.GetVersionNumber().Value.ShouldBe(numberOfVersionNumbers);
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntitySnapshots))]
    private Task
        GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsTrue_WhenRunningEntitySnapshotTransactionSubscriber_ThenAlwaysWriteSnapshot(
            TransactionsAdder transactionsAdder, SnapshotAdder entitySnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entitySnapshotAdder.SnapshotType },
            new object?[] { transactionsAdder, entitySnapshotAdder }
        );
    }

    private async Task
        Generic_GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsFalse_WhenRunningEntitySnapshotTransactionSubscriber_ThenNeverWriteSnapshot<
            TEntity>(TransactionsAdder transactionsAdder, SnapshotAdder snapshotAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        TEntity.ShouldRecordAsLatestLogic.Value = (_, _) => false;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            snapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        var transaction = TransactionSeeder.Create<TEntity>(entityId, 10);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.Write);

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.ReadOnly);

        // ACT

        await entityRepository.PutTransaction(transaction);

        var snapshot = await snapshotRepository.GetSnapshotOrDefault(entityId);

        // ASSERT

        snapshot.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntitySnapshots))]
    private Task
        GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsFalse_WhenRunningEntitySnapshotTransactionSubscriber_ThenNeverWriteSnapshot(
            TransactionsAdder transactionsAdder, SnapshotAdder entitySnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entitySnapshotAdder.SnapshotType },
            new object?[] { transactionsAdder, entitySnapshotAdder }
        );
    }
}