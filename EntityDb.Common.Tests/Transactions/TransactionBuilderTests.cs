using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Facts;
using EntityDb.Common.Transactions;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Facts;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Security.Claims;
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
                .Setup(strategy => strategy.IsAuthorized(It.IsAny<TransactionEntity>(), It.IsAny<ICommand<TransactionEntity>>(), It.IsAny<ClaimsPrincipal>()))
                .Returns(false);

            var authorizingStrategy = authorizingStrategyMock.Object;

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory(new IFact<TransactionEntity>[] { new NothingDone(), new VersionNumberSet<TransactionEntity>(1) }));

                serviceCollection.AddScoped((serviceProvider) => authorizingStrategy);
            });

            var transactionBuilder = new TransactionBuilder<TransactionEntity>(default!, serviceProvider);

            await using var transactionRepository = await serviceProvider.CreateTransactionRepository<TransactionEntity>(new TransactionSessionOptions());

            // ACT

            await transactionBuilder.Load(entityId, transactionRepository);

            // ASSERT

            Assert.Throws<CommandNotAuthorizedException>(() =>
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
                .Setup(strategy => strategy.IsAuthorized(It.IsAny<TransactionEntity>(), It.IsAny<ICommand<TransactionEntity>>(), It.IsAny<ClaimsPrincipal>()))
                .Returns(false);

            var authorizingStrategy = authorizingStrategyMock.Object;

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddScoped((serviceProvider) => GetMockedTransactionRepositoryFactory(new IFact<TransactionEntity>[] { new NothingDone() }));

                serviceCollection.AddScoped((serviceProvider) => authorizingStrategy);
            });

            var transactionBuilder = new TransactionBuilder<TransactionEntity>(default!, serviceProvider);

            // ASSERT

            Assert.Throws<CommandNotAuthorizedException>(() =>
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

            var transactionBuilder = new TransactionBuilder<TransactionEntity>(default!, serviceProvider);

            // ASSERT

            Assert.Throws<EntityNotLoadedException>(() =>
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

            var transactionBuilder = new TransactionBuilder<TransactionEntity>(default!, serviceProvider);

            // ACT

            transactionBuilder.Create(entityId, new DoNothing());

            // ASSERT

            Assert.Throws<EntityAlreadyCreatedException>(() =>
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

            var transactionBuilder = new TransactionBuilder<TransactionEntity>(default!, serviceProvider);

            await using var transactionRepository = await serviceProvider.CreateTransactionRepository<TransactionEntity>(default!);

            // ACT

            await transactionBuilder.Load(entityId, transactionRepository);

            // ASSERT

            await Assert.ThrowsAsync<EntityAlreadyLoadedException>(async () =>
            {
                await transactionBuilder.Load(entityId, transactionRepository);
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

            var transactionBuilder = new TransactionBuilder<TransactionEntity>(default!, serviceProvider);

            await using var transactionRepository = await serviceProvider.CreateTransactionRepository<TransactionEntity>(default!);

            // ASSERT

            await Assert.ThrowsAsync<EntityNotCreatedException>(async () =>
            {
                await transactionBuilder.Load(Guid.NewGuid(), transactionRepository);
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

            var transactionBuilder = new TransactionBuilder<TransactionEntity>(default!, serviceProvider);

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
                Assert.Equal(i, transaction.Commands[i].ExpectedPreviousVersionNumber);
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

            var transactionBuilder = new TransactionBuilder<TransactionEntity>(default!, serviceProvider);

            await using var transactionRepository = await serviceProvider.CreateTransactionRepository<TransactionEntity>(default!);

            // ACT

            var entity = await serviceProvider.GetEntity(entityId, transactionRepository);

            await transactionBuilder.Load(entityId, transactionRepository);

            transactionBuilder.Append(entityId, new DoNothing());

            var transaction = transactionBuilder.Build(default!, default!);

            // ASSERT

            Assert.Single(transaction.Commands);

            Assert.Equal(new DoNothing(), transaction.Commands[0].Command);
        }
    }
}
