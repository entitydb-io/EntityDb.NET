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
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Projections;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
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
            .CreateRepository(TestSessionOptions.Write, TestSessionOptions.Write, default);
        
        // ACT
        
        // ASSERT
    }

    private async Task
        Generic_GivenProjectionStrategyReturnsNoEntityIds_WhenGettingProjection_ThenReturnDefaultProjection<TProjection>(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
        where TProjection : IProjection<TProjection>
    {
        // ARRANGE

        var projectionId = Id.NewId();
        var expectedProjection = TProjection.Construct(projectionId);

        var mockProjectionStrategy = new Mock<IProjectionStrategy<TProjection>>();

        mockProjectionStrategy
            .Setup(strategy => strategy.GetEntityIds(It.IsAny<Id>(), It.IsAny<TProjection>()))
            .ReturnsAsync(Array.Empty<Id>());

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.Add(serviceCollection);
            snapshotsAdder.Add(serviceCollection);
            
            serviceCollection.RemoveAll(typeof(IProjectionStrategy<>));

            serviceCollection.AddSingleton(mockProjectionStrategy.Object);
        });

        var projectionStrategy = serviceScope.ServiceProvider
            .GetRequiredService<IProjectionStrategy<TProjection>>();
        
        await using var projectionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IProjectionRepositoryFactory<OneToOneProjection>>()
            .CreateRepository(TestSessionOptions.Write, TestSessionOptions.Write, default);

        // ACT

        var actualEntityIds = await projectionStrategy.GetEntityIds(projectionId, default!);
        
        var actualProjection = await projectionRepository.GetCurrent(projectionId, default);
        
        // ASSERT
        
        actualEntityIds.ShouldBeEmpty();
        
        actualProjection.ShouldBeEquivalentTo(expectedProjection);
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
            .CreateRepository(TestSessionOptions.Write, TestSessionOptions.Write, default);
        
        // ACT

        var actualProjection = await projectionRepository.GetCurrent(projectionId, default);
        
        // ASSERT
        
        actualProjection.ShouldBeEquivalentTo(expectedProjection);
    }
    
    
    private async Task Generic_GivenTransactionCommitted_WhenGettingProjection_ThenReturnExpectedProjection<TEntity, TProjection>(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
        where TEntity : IEntity<TEntity>, IEntityWithVersionNumber<TEntity>
        where TProjection : IProjection<TProjection>, ISnapshotWithShouldReplaceLogic<TProjection>
    {
        // ARRANGE

        const uint numberOfVersionNumbers = 5;
        const uint replaceAtVersionNumber = 3;
        
        TProjection.ShouldReplaceLogic.Value = (projection, _) => projection.GetEntityVersionNumber(default) == new VersionNumber(replaceAtVersionNumber);
        
        var projectionId = Id.NewId();
        var transaction = TransactionSeeder.Create<TEntity>(projectionId, numberOfVersionNumbers);
        
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
            .CreateRepository(TestSessionOptions.Write, TestSessionOptions.Write, default);


        var transactionInserted = await entityRepository.PutTransaction(transaction);
        
        // ARRANGE ASSERTIONS

        numberOfVersionNumbers.ShouldBeGreaterThan(replaceAtVersionNumber);
        
        transactionInserted.ShouldBeTrue();

        projectionRepository.SnapshotRepository.ShouldNotBeNull();

        // ACT

        var currentProjection = await projectionRepository.GetCurrent(projectionId, default);
        var projectionSnapshot = await projectionRepository.SnapshotRepository.GetSnapshot(projectionId);
        
        // ASSERT
        
        currentProjection.GetEntityVersionNumber(default).Value.ShouldBe(numberOfVersionNumbers);
        projectionSnapshot.ShouldNotBeNull().GetEntityVersionNumber(default).Value.ShouldBe(replaceAtVersionNumber);
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndOneToOneProjectionSnapshots))]
    public async Task GivenEmptyTransactionRepository_WhenGettingProjection_ThenReturnDefaultProjection(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(Generic_GivenEmptyTransactionRepository_WhenGettingProjection_ThenReturnDefaultProjection), ~BindingFlags.Public)!
            .MakeGenericMethod(snapshotsAdder.SnapshotType)
            .Invoke(this, new object?[] { transactionsAdder, snapshotsAdder })
            .ShouldBeAssignableTo<Task>().ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndOneToOneProjectionSnapshots))]
    public async Task GivenProjectionStrategyReturnsNoEntityIds_WhenGettingProjection_ThenReturnDefaultProjection(
        TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(Generic_GivenProjectionStrategyReturnsNoEntityIds_WhenGettingProjection_ThenReturnDefaultProjection), ~BindingFlags.Public)!
            .MakeGenericMethod(snapshotsAdder.SnapshotType)
            .Invoke(this, new object?[] { transactionsAdder, snapshotsAdder })
            .ShouldBeAssignableTo<Task>().ShouldNotBeNull();
    }
    
    [Theory]
    [MemberData(nameof(AddTransactionsAndOneToOneProjectionSnapshots))]
    public async Task GivenTransactionCommitted_WhenGettingProjection_ThenReturnExpectedProjection(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(Generic_GivenTransactionCommitted_WhenGettingProjection_ThenReturnExpectedProjection), ~BindingFlags.Public)!
            .MakeGenericMethod(transactionsAdder.EntityType, snapshotsAdder.SnapshotType)
            .Invoke(this, new object?[] { transactionsAdder, snapshotsAdder })
            .ShouldBeAssignableTo<Task>().ShouldNotBeNull();
    }
}