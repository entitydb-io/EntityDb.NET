using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Transactions.Builders;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Leases;
using EntityDb.Common.Tests.Implementations.Commands;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Transactions;

public class SingleEntityTransactionBuilderTests : TestsBase<Startup>
{
    public SingleEntityTransactionBuilderTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    private async Task Generic_GivenEntityNotKnown_WhenGettingEntity_ThenThrow<TEntity>(EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default, default);

        // ASSERT

        transactionBuilder.IsEntityKnown().ShouldBeFalse();

        Should.Throw<KeyNotFoundException>(() => transactionBuilder.GetEntity());
    }

    private async Task Generic_GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity<TEntity>(EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedEntityId = Id.NewId();

        var expectedEntity = TEntity
            .Construct(expectedEntityId);

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, expectedEntityId, default);

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

    private async Task Generic_GivenLeasingStrategy_WhenBuildingNewEntityWithLease_ThenTransactionDoesInsertLeases<TEntity>(EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default, default);

        // ACT

        var transaction = transactionBuilder
            .Add(new Lease(default!, default!, default!))
            .Build(default);

        // ASSERT

        transaction.Steps.Length.ShouldBe(1);

        var leaseTransactionStep = transaction.Steps[0].ShouldBeAssignableTo<IAddLeasesTransactionStep>().ShouldNotBeNull();

        leaseTransactionStep.Leases.ShouldNotBeEmpty();
    }

    private async Task Generic_GivenExistingEntityId_WhenUsingEntityIdForLoadTwice_ThenLoadThrows<TEntity>(EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddScoped(_ =>
                GetMockedTransactionRepositoryFactory(
                    new object[] { new DoNothing() }));
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default, default);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(default!);

        var entity = await entityRepository.GetSnapshot(entityId);

        // ACT

        transactionBuilder.Load(entity);

        // ASSERT

        Should.Throw<EntityAlreadyKnownException>(() =>
        {
            transactionBuilder.Load(entity);
        });
    }

    private async Task Generic_GivenNonExistingEntityId_WhenUsingValidVersioningStrategy_ThenVersionNumberAutoIncrements<TEntity>(EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var numberOfVersionsToTest = new VersionNumber(10);

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddScoped(_ =>
                GetMockedTransactionRepositoryFactory());
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default, default);

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

            var commandTransactionStep = transaction.Steps[index].ShouldBeAssignableTo<IAppendCommandTransactionStep>().ShouldNotBeNull();

            commandTransactionStep.EntityVersionNumber.ShouldBe(v);
        }
    }

    private async Task Generic_GivenExistingEntity_WhenAppendingNewCommand_ThenTransactionBuilds<TEntity>(EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var entityId = Id.NewId();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddScoped(_ =>
                GetMockedTransactionRepositoryFactory(new object[] { new DoNothing() }));
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default, default);

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(default!);

        var entity = await entityRepository.GetSnapshot(entityId);

        // ACT

        var transaction = transactionBuilder
            .Load(entity)
            .Append(new DoNothing())
            .Build(default);

        // ASSERT

        transaction.Steps.Length.ShouldBe(1);

        var commandTransactionStep = transaction.Steps[0].ShouldBeAssignableTo<IAppendCommandTransactionStep>().ShouldNotBeNull();

        commandTransactionStep.Command.ShouldBeEquivalentTo(new DoNothing());
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenEntityNotKnown_WhenGettingEntity_ThenThrow(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenLeasingStrategy_WhenBuildingNewEntityWithLease_ThenTransactionDoesInsertLeases(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenExistingEntityId_WhenUsingEntityIdForLoadTwice_ThenLoadThrows(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenNonExistingEntityId_WhenUsingValidVersioningStrategy_ThenVersionNumberAutoIncrements(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenExistingEntity_WhenAppendingNewCommand_ThenTransactionBuilds(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }
}