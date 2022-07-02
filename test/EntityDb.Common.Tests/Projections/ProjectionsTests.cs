using System;
using System.Threading.Tasks;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
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
    
    private async Task Generic_GivenEmptyTransactionRepository_WhenGettingProjection_ThenThrow<TProjection>(TransactionsAdder transactionsAdder, SnapshotAdder entitySnapshotAdder, SnapshotAdder projectionSnapshotAdder)
        where TProjection : IProjection<TProjection>
    {
        // ARRANGE

        var projectionId = Id.NewId();
        var expectedProjection = TProjection.Construct(projectionId);
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entitySnapshotAdder.AddDependencies.Invoke(serviceCollection);
            projectionSnapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var projectionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IProjectionRepositoryFactory<OneToOneProjection>>()
            .CreateRepository(TestSessionOptions.Write, TestSessionOptions.Write, default);
        
        // ACT & ASSERT

        await Should.ThrowAsync<SnapshotPointernDoesNotExistException>(() => projectionRepository.GetSnapshot(projectionId, default));
    }
    
    
    private async Task Generic_GivenTransactionCommitted_WhenGettingProjection_ThenReturnExpectedProjection<TEntity, TProjection>(TransactionsAdder transactionsAdder, SnapshotAdder entitySnapshotAdder, SnapshotAdder projectionSnapshotAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
        where TProjection : IProjection<TProjection>, ISnapshotWithTestLogic<TProjection>
    {
        // ARRANGE

        const uint numberOfVersionNumbers = 5;
        const uint replaceAtVersionNumber = 3;

        TProjection.ShouldRecordAsLatestLogic.Value = (projection, _) => projection.GetVersionNumber() == new VersionNumber(replaceAtVersionNumber);
        
        var projectionId = Id.NewId();
        var transaction = TransactionSeeder.Create<TEntity>(projectionId, numberOfVersionNumbers);
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entitySnapshotAdder.AddDependencies.Invoke(serviceCollection);
            projectionSnapshotAdder.AddDependencies.Invoke(serviceCollection);
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
    [MemberData(nameof(AddTransactionsEntitySnapshotsAndProjectionSnapshots))]
    public Task GivenEmptyTransactionRepository_WhenGettingProjection_ThenThrow(TransactionsAdder transactionsAdder, SnapshotAdder entitySnapshotAdder, SnapshotAdder projectionSnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { projectionSnapshotAdder.SnapshotType },
            new object?[] { transactionsAdder, entitySnapshotAdder, projectionSnapshotAdder }
        );
    }
    
    [Theory]
    [MemberData(nameof(AddTransactionsEntitySnapshotsAndProjectionSnapshots))]
    public Task GivenTransactionCommitted_WhenGettingProjection_ThenReturnExpectedProjection(TransactionsAdder transactionsAdder, SnapshotAdder entitySnapshotAdder, SnapshotAdder projectionSnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entitySnapshotAdder.SnapshotType, projectionSnapshotAdder.SnapshotType },
            new object?[] { transactionsAdder, entitySnapshotAdder, projectionSnapshotAdder }
        );
    }
}