using System;
using System.Reflection;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.Common.Transactions;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Transactions;

public class EntitySnapshotTransactionSubscriberTests : TestsBase<Startup>
{
    public EntitySnapshotTransactionSubscriberTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }
    
    
    private async Task
        Generic_GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsTrue_WhenRunningEntitySnapshotTransactionSubscriber_ThenAlwaysWriteSnapshot<TEntity>(
            TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestMethods<TEntity>
    {
        // ARRANGE

        TEntity.ShouldRecordAsMostRecentLogic.Value = (_, _) => true;
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.Add(serviceCollection);
            snapshotsAdder.Add(serviceCollection);
        });

        var subscriber =
            EntitySnapshotTransactionSubscriber<TEntity>.Create(serviceScope.ServiceProvider,
                TestSessionOptions.Write, true);

        var entityId = Id.NewId();

        const uint numberOfVersionNumbers = 10;
        
        var transaction = TransactionSeeder.Create<TEntity>(entityId, numberOfVersionNumbers);

        await using var snapshotRepository = await subscriber.CreateSnapshotRepository();

        // ACT
        
        subscriber.Notify(transaction);

        var snapshot = await snapshotRepository.GetSnapshot(entityId);

        // ASSERT
        
        snapshot.ShouldNotBe(default);
        snapshot!.GetVersionNumber().Value.ShouldBe(numberOfVersionNumbers);
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntitySnapshots))]
    private async Task
        GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsTrue_WhenRunningEntitySnapshotTransactionSubscriber_ThenAlwaysWriteSnapshot(
            TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(Generic_GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsTrue_WhenRunningEntitySnapshotTransactionSubscriber_ThenAlwaysWriteSnapshot), ~BindingFlags.Public)!
            .MakeGenericMethod(transactionsAdder.EntityType)
            .Invoke(this, new object?[] { transactionsAdder, snapshotsAdder })
            .ShouldBeAssignableTo<Task>().ShouldNotBeNull();
    }
    
    private async Task
        Generic_GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsFalse_WhenRunningEntitySnapshotTransactionSubscriber_ThenNeverWriteSnapshot<TEntity>(
            TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestMethods<TEntity>
    {
        // ARRANGE

        TEntity.ShouldRecordAsMostRecentLogic.Value = (_, _) => false;
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.Add(serviceCollection);
            snapshotsAdder.Add(serviceCollection);
        });

        var subscriber =
            EntitySnapshotTransactionSubscriber<TEntity>.Create(serviceScope.ServiceProvider,
                TestSessionOptions.Write, true);

        var entityId = Id.NewId();

        var transaction = TransactionSeeder.Create<TEntity>(entityId, 10);

        await using var snapshotRepository = await subscriber.CreateSnapshotRepository();

        // ACT
        
        subscriber.Notify(transaction);

        var snapshot = await snapshotRepository.GetSnapshot(entityId);

        // ASSERT
        
        snapshot.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntitySnapshots))]
    private async Task
        GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsFalse_WhenRunningEntitySnapshotTransactionSubscriber_ThenNeverWriteSnapshot(
            TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(Generic_GivenSnapshotShouldRecordAsMostRecentAlwaysReturnsFalse_WhenRunningEntitySnapshotTransactionSubscriber_ThenNeverWriteSnapshot), ~BindingFlags.Public)!
            .MakeGenericMethod(transactionsAdder.EntityType)
            .Invoke(this, new object?[] { transactionsAdder, snapshotsAdder })
            .ShouldBeAssignableTo<Task>().ShouldNotBeNull();
    }
}