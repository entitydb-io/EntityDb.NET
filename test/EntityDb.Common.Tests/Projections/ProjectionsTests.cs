using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Projections;
using EntityDb.Common.Tests.Implementations.Projections;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Projections;

public class ProjectionsTests : TestsBase<Startup>
{
    public ProjectionsTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }
    
    private async Task Generic_Given_When_Then<TProjection>(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
        where TProjection : IProjection<TProjection>
    {
        // ARRANGE 
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.Add(serviceCollection);
            snapshotsAdder.Add(serviceCollection);
        });

        await using var projectionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IProjectionRepositoryFactory<OneToOneProjection>>()
            .CreateRepository(TestSessionOptions.Write, TestSessionOptions.Write);
        
        // ACT
        
        // ASSERT
    }
    
    private async Task Generic_GivenEmptyTransactionRepository_WhenGettingProjection_ThenReturnDefaultProjection<TProjection>(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
        where TProjection : IProjection<TProjection>
    {
        // ARRANGE

        var projectionId = Id.NewId();
        var expectedProjection = TProjection.Construct(projectionId);
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.Add(serviceCollection);
            snapshotsAdder.Add(serviceCollection);
        });

        await using var projectionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IProjectionRepositoryFactory<OneToOneProjection>>()
            .CreateRepository(TestSessionOptions.Write, TestSessionOptions.Write);
        
        // ACT

        var actualProjection = await projectionRepository.GetCurrent(projectionId);
        
        // ASSERT
        
        actualProjection.ShouldBeEquivalentTo(expectedProjection);
    }
    
    
    private async Task Generic_GivenTransactionCommitted_WhenGettingProject_ThenReturnExpectedProjection<TEntity, TProjection>(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
        where TEntity : IEntity<TEntity>
        where TProjection : IProjection<TProjection>, ISnapshotWithShouldReplaceLogic<TProjection>
    {
        // ARRANGE

        const uint NumberOfVersionNumbers = 5;
        const uint ReplaceAtVersionNumber = 3;
        
        TProjection.ShouldReplaceLogic.Value = (projection, _) => projection.GetEntityVersionNumber(default) == new VersionNumber(ReplaceAtVersionNumber);
        
        var projectionId = Id.NewId();
        var transaction = TransactionSeeder.Create(projectionId, NumberOfVersionNumbers);
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.Add(serviceCollection);
            snapshotsAdder.Add(serviceCollection);
        });

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.Write);
        
        await using var projectionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IProjectionRepositoryFactory<OneToOneProjection>>()
            .CreateRepository(TestSessionOptions.Write, TestSessionOptions.Write);

        var transactionInserted = await entityRepository.PutTransaction(transaction);
        
        // ARRANGE ASSERTIONS

        NumberOfVersionNumbers.ShouldBeGreaterThan(ReplaceAtVersionNumber);
        
        transactionInserted.ShouldBeTrue();
        
        // ACT

        var currentProjection = await projectionRepository.GetCurrent(projectionId);
        var projectionSnapshot = await projectionRepository.SnapshotRepository.GetSnapshot(projectionId);
        
        // ASSERT
        
        currentProjection.GetEntityVersionNumber(default).Value.ShouldBe(NumberOfVersionNumbers);
        projectionSnapshot.ShouldNotBeNull().GetEntityVersionNumber(default).Value.ShouldBe(ReplaceAtVersionNumber);
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndOneToOneProjectionSnapshots))]
    public async Task GivenEmptyTransactionRepository_WhenGettingProjection_ThenReturnDefaultProjection(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(Generic_GivenEmptyTransactionRepository_WhenGettingProjection_ThenReturnDefaultProjection), ~BindingFlags.Public)!
            .MakeGenericMethod(snapshotsAdder.SnapshotType)
            .Invoke(this, new object?[] { transactionsAdder, snapshotsAdder })
            .ShouldBeAssignableTo<Task>()!;
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndOneToOneProjectionSnapshots))]
    public async Task GivenTransactionCommitted_WhenGettingProject_ThenReturnExpectedProjection(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(Generic_GivenTransactionCommitted_WhenGettingProject_ThenReturnExpectedProjection), ~BindingFlags.Public)!
            .MakeGenericMethod(transactionsAdder.EntityType, snapshotsAdder.SnapshotType)
            .Invoke(this, new object?[] { transactionsAdder, snapshotsAdder })
            .ShouldBeAssignableTo<Task>()!;
    }

}