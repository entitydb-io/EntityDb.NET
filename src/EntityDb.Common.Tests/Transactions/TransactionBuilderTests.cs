using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.Transactions
{
    public class TransactionBuilderTests : TestsBase<Startup>
    {
        public TransactionBuilderTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [Fact]
        public void GivenNoAuthorizingStrategy_WhenExecutingUnauthorizedCommand_ThenBuildSucceeds()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.RemoveAll(typeof(IAuthorizingStrategy<TransactionEntity>));
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            var entityId = Guid.NewGuid();

            // ACT & ASSERT

            Should.NotThrow(() => transactionBuilder
                .Create(entityId, new SetRole("Foo"))
                .Append(entityId, new DoNothing()));
        }

        [Fact]
        public async Task GivenExistingEntityId_WhenExecutingUnauthorizedCommand_ThenAppendThrows()
        {
            // ARRANGE

            var authorizingStrategyMock = new Mock<IAuthorizingStrategy<TransactionEntity>>(MockBehavior.Strict);

            authorizingStrategyMock
                .Setup(strategy => strategy.IsAuthorized(It.IsAny<TransactionEntity>(),
                    It.IsAny<ICommand<TransactionEntity>>()))
                .Returns(false);

            var authorizingStrategy = authorizingStrategyMock.Object;

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory(new ICommand<TransactionEntity>[] { new DoNothing() }));

                serviceCollection.AddScoped(_ => authorizingStrategy);
            });

            var entityId = Guid.NewGuid();

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            await using var entityRepository = await serviceScope.ServiceProvider
                .GetRequiredService<IEntityRepositoryFactory<TransactionEntity>>()
                    .CreateRepository(default!);

            // ACT

            await transactionBuilder.Load(entityId, entityRepository);

            // ASSERT

            Should.Throw<CommandNotAuthorizedException>(() =>
            {
                transactionBuilder.Append(entityId, new DoNothing());
            });
        }

        [Fact]
        public void GivenNonExistingEntityId_WhenExecutingUnauthorizedCommand_ThenCreateThrows()
        {
            // ARRANGE

            var entityId = Guid.NewGuid();

            var authorizingStrategyMock = new Mock<IAuthorizingStrategy<TransactionEntity>>(MockBehavior.Strict);

            authorizingStrategyMock
                .Setup(strategy => strategy.IsAuthorized(It.IsAny<TransactionEntity>(),
                    It.IsAny<ICommand<TransactionEntity>>()))
                .Returns(false);

            var authorizingStrategy = authorizingStrategyMock.Object;

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory(new ICommand<TransactionEntity>[] { new DoNothing() }));

                serviceCollection.AddScoped(_ => authorizingStrategy);
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            // ASSERT

            Should.Throw<CommandNotAuthorizedException>(() =>
            {
                transactionBuilder.Create(entityId, new DoNothing());
            });
        }

        [Fact]
        public void GivenNonExistingEntityId_WhenUsingEntityIdForAppend_ThenAppendThrows()
        {
            // ARRANGE

            var entityId = Guid.NewGuid();

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory());
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            // ASSERT

            Should.Throw<EntityNotLoadedException>(() =>
            {
                transactionBuilder.Append(entityId, new DoNothing());
            });
        }

        [Fact]
        public void GivenExistingEntityId_WhenUsingEntityIdForCreate_ThenCreateThrows()
        {
            // ARRANGE

            var entityId = Guid.NewGuid();

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory());
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            // ACT

            transactionBuilder.Create(entityId, new DoNothing());

            // ASSERT

            Should.Throw<EntityAlreadyCreatedException>(() =>
            {
                transactionBuilder.Create(entityId, new DoNothing());
            });
        }

        [Fact]
        public void GivenLeasingStrategy_WhenBuildingNewEntityWithLease_ThenTransactionDoesInsertLeases()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            // ACT

            var transaction = transactionBuilder
                .Create(default, new AddLease(default!, default!, default!))
                .Build(default);

            // ASSERT

            transaction.Steps.Length.ShouldBe(1);

            transaction.Steps[0].Leases.Insert.ShouldNotBeEmpty();
        }

        [Fact]
        public void GivenNoLeasingStrategy_WhenBuildingNewEntityWithLease_ThenTransactionDoesNotInsertLeases()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.RemoveAll(typeof(ILeasingStrategy<TransactionEntity>));
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            // ACT

            var transaction = transactionBuilder
                .Create(default, new AddLease(default!, default!, default!))
                .Build(default);

            // ASSERT

            transaction.Steps.Length.ShouldBe(1);

            transaction.Steps[0].Leases.Insert.ShouldBeEmpty();
        }

        [Fact]
        public void GivenNoTaggingStrategy_WhenBuildingNewEntityWithTag_ThenTransactionDoesNotInsertTags()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.RemoveAll(typeof(ITaggingStrategy<TransactionEntity>));
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            // ACT

            var transaction = transactionBuilder
                .Create(default, new AddTag(default!, default!))
                .Build(default);

            // ASSERT

            transaction.Steps.Length.ShouldBe(1);

            transaction.Steps[0].Tags.Insert.ShouldBeEmpty();
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
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            await using var entityRepository = await serviceScope.ServiceProvider
                .GetRequiredService<IEntityRepositoryFactory<TransactionEntity>>()
                    .CreateRepository(default!);

            // ACT

            await transactionBuilder.Load(entityId, entityRepository);

            // ASSERT

            await Should.ThrowAsync<EntityAlreadyLoadedException>(async () =>
            {
                await transactionBuilder.Load(entityId, entityRepository);
            });
        }

        [Fact]
        public async Task GivenNonExistentEntityId_WhenLoadingEntity_ThenLoadThrows()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddScoped(_ =>
                    GetMockedTransactionRepositoryFactory());
            });

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            await using var entityRepository = await serviceScope.ServiceProvider
                .GetRequiredService<IEntityRepositoryFactory<TransactionEntity>>()
                    .CreateRepository(default!);

            // ASSERT

            await Should.ThrowAsync<EntityNotCreatedException>(async () =>
            {
                await transactionBuilder.Load(Guid.NewGuid(), entityRepository);
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
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            // ACT

            transactionBuilder.Create(entityId, new DoNothing());

            for (ulong i = 1; i < NumberOfVersionsToTest; i++)
            {
                transactionBuilder.Append(entityId, new DoNothing());
            }

            var transaction = transactionBuilder.Build(default);

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
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            await using var entityRepository = await serviceScope.ServiceProvider
                .GetRequiredService<IEntityRepositoryFactory<TransactionEntity>>()
                    .CreateRepository(default!);

            // ACT

            await transactionBuilder.Load(entityId, entityRepository);

            transactionBuilder.Append(entityId, new DoNothing());

            var transaction = transactionBuilder.Build(default);

            // ASSERT

            transaction.Steps.Length.ShouldBe(1);

            transaction.Steps[0].Command.ShouldBeEquivalentTo(new DoNothing());
        }
    }
}
