using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Leases;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using Xunit;
using EntityDb.Abstractions.Transactions.Builders;

namespace EntityDb.Common.Tests.Transactions;

public class SingleEntityTransactionBuilderTests : TestsBase<Startup>
{
    public SingleEntityTransactionBuilderTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Fact]
    public async Task GivenEntityNotKnown_WhenGettingEntity_ThenThrow()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TestEntity>>()
            .CreateForSingleEntity(default!, default);

        // ASSERT

        transactionBuilder.IsEntityKnown().ShouldBeFalse();

        Should.Throw<KeyNotFoundException>(() => transactionBuilder.GetEntity());
    }

    [Fact]
    public async Task GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var expectedEntityId = Id.NewId();

        var expectedEntity = TestEntity
            .Construct(expectedEntityId);

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TestEntity>>()
            .CreateForSingleEntity(default!, expectedEntityId);

        transactionBuilder.Load(expectedEntity);

        // ARRANGE ASSERTIONS

        transactionBuilder.IsEntityKnown().ShouldBeTrue();

        // ACT

        var actualEntityId = transactionBuilder.EntityId;
        var actualEntity = transactionBuilder.GetEntity();

        // ASSERT

        actualEntityId.ShouldBe(expectedEntityId);
        actualEntity.ShouldBe(expectedEntity);
    }

    [Fact]
    public async Task GivenLeasingStrategy_WhenBuildingNewEntityWithLease_ThenTransactionDoesInsertLeases()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TestEntity>>()
            .CreateForSingleEntity(default!, default);

        // ACT

        var transaction = transactionBuilder
            .Add(new Lease(default!, default!, default!))
            .Build(default);

        // ASSERT

        transaction.Steps.Length.ShouldBe(1);
            
        var leaseTransactionStep = transaction.Steps[0].ShouldBeAssignableTo<IAddLeasesTransactionStep>()!;

        leaseTransactionStep.Leases.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GivenExistingEntityId_WhenUsingEntityIdForLoadTwice_ThenLoadThrows()
    {
        // ARRANGE

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddScoped(_ =>
                GetMockedTransactionRepositoryFactory(
                    new object[] { new DoNothing() }));
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TestEntity>>()
            .CreateForSingleEntity(default!, default);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TestEntity>>()
            .CreateRepository(default!);

        var entity = await entityRepository.GetCurrent(entityId);

        // ACT

        transactionBuilder.Load(entity);

        // ASSERT

        Should.Throw<EntityAlreadyKnownException>(() =>
        {
            transactionBuilder.Load(entity);
        });
    }

    [Fact]
    public async Task GivenNonExistingEntityId_WhenUsingValidVersioningStrategy_ThenVersionNumberAutoIncrements()
    {
        // ARRANGE

        var numberOfVersionsToTest = new VersionNumber(10);

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddScoped(_ =>
                GetMockedTransactionRepositoryFactory());
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TestEntity>>()
            .CreateForSingleEntity(default!, default);

        // ACT

        for (var i = new VersionNumber(1); i.Value <= numberOfVersionsToTest.Value; i = i.Next())
        {
            transactionBuilder.Append(new DoNothing());
        }

        var transaction = transactionBuilder.Build(default);

        // ASSERT

        for (var v = new VersionNumber(1); v.Value <= numberOfVersionsToTest.Value; v = v.Next())
        {
            var index = (int)(v.Value - 1);

            var commandTransactionStep = transaction.Steps[index].ShouldBeAssignableTo<IAppendCommandTransactionStep>()!;

            commandTransactionStep.EntityVersionNumber.ShouldBe(v);
        }
    }

    [Fact]
    public async Task GivenExistingEntity_WhenAppendingNewCommand_ThenTransactionBuilds()
    {
        // ARRANGE

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddScoped(_ =>
                GetMockedTransactionRepositoryFactory(new object[] { new DoNothing() }));
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TestEntity>>()
            .CreateForSingleEntity(default!, default);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TestEntity>>()
            .CreateRepository(default!);

        var entity = await entityRepository.GetCurrent(entityId);

        // ACT

        var transaction = transactionBuilder
            .Load(entity)
            .Append(new DoNothing())
            .Build(default);

        // ASSERT

        transaction.Steps.Length.ShouldBe(1);

        var commandTransactionStep = transaction.Steps[0].ShouldBeAssignableTo<IAppendCommandTransactionStep>()!;

        commandTransactionStep.Command.ShouldBeEquivalentTo(new DoNothing());
    }
}