using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.Transactions
{
    public class SingleEntityTransactionBuilderTests : TestsBase<Startup>
    {
        public SingleEntityTransactionBuilderTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [Fact]
        public void GivenEntityNotKnown_WhenGettingEntity_ThenThrow()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .ForSingleEntity(default);

            // ASSERT

            transactionBuilder.IsEntityKnown().ShouldBeFalse();

            Should.Throw<KeyNotFoundException>(() => transactionBuilder.GetEntity());
        }

        [Fact]
        public void GivenEntityKnown_WhenGettingEntity_ThenReturnExpectedEntity()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var expectedEntityId = Guid.NewGuid();

            var expectedEntity = TransactionEntity
                .Construct(expectedEntityId);

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .ForSingleEntity(expectedEntityId);

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
        public void GivenNonExistingEntityId_WhenUsingEntityIdForAppend_ThenAppendThrows()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory());
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .ForSingleEntity(default);

            // ASSERT

            Should.Throw<EntityNotLoadedException>(() =>
            {
                transactionBuilder.Append(new DoNothing());
            });
        }

        [Fact]
        public void GivenExistingEntityId_WhenUsingEntityIdForCreate_ThenCreateThrows()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory());
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .ForSingleEntity(default);

            // ACT

            transactionBuilder.Create(new DoNothing());

            // ASSERT

            Should.Throw<EntityAlreadyCreatedException>(() =>
            {
                transactionBuilder.Create(new DoNothing());
            });
        }

        [Fact]
        public void GivenLeasingStrategy_WhenBuildingNewEntityWithLease_ThenTransactionDoesInsertLeases()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .ForSingleEntity(default);

            // ACT

            var transaction = transactionBuilder
                .Create(new AddLease(default!, default!, default!))
                .Build(default!, default);

            // ASSERT

            transaction.Steps.Length.ShouldBe(1);

            transaction.Steps[0].Leases.Insert.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task GivenExistingEntityId_WhenUsingEntityIdForLoadTwice_ThenLoadThrows()
        {
            // ARRANGE

            var entityId = Guid.NewGuid();

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory(
                        new ICommand<TransactionEntity>[] { new DoNothing() }));
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .ForSingleEntity(entityId);

            await using var entityRepository = await serviceScope.ServiceProvider
                .GetRequiredService<IEntityRepositoryFactory<TransactionEntity>>()
                    .CreateRepository(default!);

            var entity = await entityRepository.GetCurrent(entityId);

            // ACT

            transactionBuilder.Load(entity);

            // ASSERT

            Should.Throw<EntityAlreadyLoadedException>(() =>
            {
                transactionBuilder.Load(entity);
            });
        }

        [Fact]
        public void GivenNonExistingEntityId_WhenUsingValidVersioningStrategy_ThenVersionNumberAutoIncrements()
        {
            // ARRANGE

            const ulong NumberOfVersionsToTest = 10;

            var entityId = Guid.NewGuid();

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory());
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .ForSingleEntity(entityId);

            // ACT

            transactionBuilder.Create(new DoNothing());

            for (ulong i = 1; i < NumberOfVersionsToTest; i++)
            {
                transactionBuilder.Append(new DoNothing());
            }

            var transaction = transactionBuilder.Build(default!, default);

            // ASSERT

            for (ulong i = 1; i <= NumberOfVersionsToTest; i++)
            {
                transaction.Steps[(int)(i - 1)].NextEntityVersionNumber.ShouldBe(i);
            }
        }

        [Fact]
        public async Task GivenExistingEntity_WhenAppendingNewCommand_ThenTransactionBuilds()
        {
            // ARRANGE

            var entityId = Guid.NewGuid();

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory(new ICommand<TransactionEntity>[] { new DoNothing() }));
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .ForSingleEntity(entityId);

            await using var entityRepository = await serviceScope.ServiceProvider
                .GetRequiredService<IEntityRepositoryFactory<TransactionEntity>>()
                    .CreateRepository(default!);

            var entity = await entityRepository.GetCurrent(entityId);

            // ACT

            var transaction = transactionBuilder
                .Load(entity)
                .Append(new DoNothing())
                .Build(default!, default);

            // ASSERT

            transaction.Steps.Length.ShouldBe(1);

            transaction.Steps[0].Command.ShouldBeEquivalentTo(new DoNothing());
        }
    }
}
