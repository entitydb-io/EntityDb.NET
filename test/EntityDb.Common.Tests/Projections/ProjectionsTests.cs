using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
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
    
    private async Task Generic_GivenEmptyTransactionRepository_WhenGettingProjection_ThenThrow<TProjection>(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
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
        
        // ACT & ASSERT

        await Should.ThrowAsync<SnapshotPointernDoesNotExistException>(() => projectionRepository.GetSnapshot(projectionId, default));
    }
    
    
    private async Task Generic_GivenTransactionCommitted_WhenGettingProjection_ThenReturnExpectedProjection<TEntity, TProjection>(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestMethods<TEntity>
        where TProjection : IProjection<TProjection>, ISnapshotWithTestMethods<TProjection>
    {
        // ARRANGE

        const uint numberOfVersionNumbers = 5;
        const uint replaceAtVersionNumber = 3;

        TProjection.ShouldRecordAsLatestLogic.Value = (projection, _) => projection.GetVersionNumber() == new VersionNumber(replaceAtVersionNumber);
        
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

        var currentProjection = await projectionRepository.GetSnapshot(projectionId, default);
        var projectionSnapshot = await projectionRepository.SnapshotRepository.GetSnapshotOrDefault(projectionId);
        
        // ASSERT
        
        currentProjection.GetVersionNumber().Value.ShouldBe(numberOfVersionNumbers);
        projectionSnapshot.ShouldNotBeNull().GetVersionNumber().Value.ShouldBe(replaceAtVersionNumber);
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndOneToOneProjectionSnapshots))]
    public async Task GivenEmptyTransactionRepository_WhenGettingProjection_ThenThrow(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(Generic_GivenEmptyTransactionRepository_WhenGettingProjection_ThenThrow), ~BindingFlags.Public)!
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