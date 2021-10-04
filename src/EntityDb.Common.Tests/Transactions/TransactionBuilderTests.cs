using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Facts;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Facts;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.Transactions
{
    public class TransactionBuilderTests : TestsBase
    {
        public TransactionBuilderTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [Fact]
        public async Task GivenExistingEntityId_WhenExecutingUnauthorizedCommand_ThenAppendThrows()
        {
            // ARRANGE

            var entityId = Guid.NewGuid();

            var authorizingStrategyMock = new Mock<IAuthorizingStrategy<TransactionEntity>>(MockBehavior.Strict);

            authorizingStrategyMock
                .Setup(strategy => strategy.IsAuthorized(It.IsAny<TransactionEntity>(), It.IsAny<ICommand<TransactionEntity>>(), It.IsAny<IAgent>()))
                .Returns(false);

            var authorizingStrategy = authorizingStrategyMock.Object;

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory(new IFact<TransactionEntity>[] { new NothingDone(), new VersionNumberSet<TransactionEntity>(1) }));

                serviceCollection.AddScoped((serviceProvider) => authorizingStrategy);
            });

            var transactionBuilder = serviceProvider.GetTransactionBuilder<TransactionEntity>();

            await using var entityRepository = await serviceProvider.CreateEntityRepository<TransactionEntity>(default!);

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
                .Setup(strategy => strategy.IsAuthorized(It.IsAny<TransactionEntity>(), It.IsAny<ICommand<TransactionEntity>>(), It.IsAny<IAgent>()))
                .Returns(false);

            var authorizingStrategy = authorizingStrategyMock.Object;

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory(new IFact<TransactionEntity>[] { new NothingDone() }));

                serviceCollection.AddScoped((serviceProvider) => authorizingStrategy);
            });

            var transactionBuilder = serviceProvider.GetTransactionBuilder<TransactionEntity>();

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

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory<TransactionEntity>());
            });

            var transactionBuilder = serviceProvider.GetTransactionBuilder<TransactionEntity>();

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

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory<TransactionEntity>());
            });

            var transactionBuilder = serviceProvider.GetTransactionBuilder<TransactionEntity>();

            // ACT

            transactionBuilder.Create(entityId, new DoNothing());

            // ASSERT

            Should.Throw<EntityAlreadyCreatedException>(() =>
            {
                transactionBuilder.Create(entityId, new DoNothing());
            });
        }

        [Fact]
        public async Task GivenExistingEntityId_WhenUsingEntityIdForLoadTwice_ThenLoadThrows()
        {
            // ARRANGE

            var entityId = Guid.NewGuid();

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory(new IFact<TransactionEntity>[] { new VersionNumberSet<TransactionEntity>(1) }));
            });

            var transactionBuilder = serviceProvider.GetTransactionBuilder<TransactionEntity>();

            await using var entityRepository = await serviceProvider.CreateEntityRepository<TransactionEntity>(default!);

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

            var entityId = Guid.NewGuid();

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory<TransactionEntity>());
            });

            var transactionBuilder = serviceProvider.GetTransactionBuilder<TransactionEntity>();

            await using var entityRepository = await serviceProvider.CreateEntityRepository<TransactionEntity>(default!);

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

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory<TransactionEntity>());
            });

            var transactionBuilder = serviceProvider.GetTransactionBuilder<TransactionEntity>();

            // ACT

            transactionBuilder.Create(entityId, new DoNothing());

            for (ulong i = 1; i < NumberOfVersionsToTest; i++)
            {
                transactionBuilder.Append(entityId, new DoNothing());
            }

            var transaction = transactionBuilder.Build(default!, default!);

            // ASSERT

            for (ulong i = 0; i < NumberOfVersionsToTest; i++)
            {
                transaction.Commands[(int)i].ExpectedPreviousVersionNumber.ShouldBe(i);
            }
        }

        [Fact]
        public async Task GivenExistingEntity_WhenAppendingNewCommand_ThenTransactionBuilds()
        {
            // ARRANGE

            var entityId = Guid.NewGuid();

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory(new IFact<TransactionEntity>[] { new NothingDone(), new VersionNumberSet<TransactionEntity>(1) }));
            });

            var transactionBuilder = serviceProvider.GetTransactionBuilder<TransactionEntity>();

            await using var entityRepository = await serviceProvider.CreateEntityRepository<TransactionEntity>(default!);

            // ACT

            var entity = await entityRepository.Get(entityId);

            await transactionBuilder.Load(entityId, entityRepository);

            transactionBuilder.Append(entityId, new DoNothing());

            var transaction = transactionBuilder.Build(default!, default!);

            // ASSERT

            transaction.Commands.Length.ShouldBe(1);

            transaction.Commands[0].Command.ShouldBeEquivalentTo(new DoNothing());
        }
    }
}
