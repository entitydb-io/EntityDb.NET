using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Facts;
using EntityDb.Common.Transactions;
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

            Guid entityId = Guid.NewGuid();

            Mock<IAuthorizingStrategy<TransactionEntity>>? authorizingStrategyMock = new(MockBehavior.Strict);

            authorizingStrategyMock
                .Setup(strategy => strategy.IsAuthorized(It.IsAny<TransactionEntity>(),
                    It.IsAny<ICommand<TransactionEntity>>(), It.IsAny<IAgent>()))
                .Returns(false);

            IAuthorizingStrategy<TransactionEntity>? authorizingStrategy = authorizingStrategyMock.Object;

            IServiceProvider? serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddScoped(serviceProvider =>
                    GetMockedTransactionRepositoryFactory(new IFact<TransactionEntity>[]
                    {
                        new NothingDone(), new VersionNumberSet<TransactionEntity>(1)
                    }));

                serviceCollection.AddScoped(serviceProvider => authorizingStrategy);
            });

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                serviceProvider.GetTransactionBuilder<TransactionEntity>();

            await using IEntityRepository<TransactionEntity>? entityRepository =
                await serviceProvider.CreateEntityRepository<TransactionEntity>(default!);

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

            Guid entityId = Guid.NewGuid();

            Mock<IAuthorizingStrategy<TransactionEntity>>? authorizingStrategyMock = new(MockBehavior.Strict);

            authorizingStrategyMock
                .Setup(strategy => strategy.IsAuthorized(It.IsAny<TransactionEntity>(),
                    It.IsAny<ICommand<TransactionEntity>>(), It.IsAny<IAgent>()))
                .Returns(false);

            IAuthorizingStrategy<TransactionEntity>? authorizingStrategy = authorizingStrategyMock.Object;

            IServiceProvider? serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddScoped(serviceProvider =>
                    GetMockedTransactionRepositoryFactory(new IFact<TransactionEntity>[] { new NothingDone() }));

                serviceCollection.AddScoped(serviceProvider => authorizingStrategy);
            });

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                serviceProvider.GetTransactionBuilder<TransactionEntity>();

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

            Guid entityId = Guid.NewGuid();

            IServiceProvider? serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddScoped(serviceProvider =>
                    GetMockedTransactionRepositoryFactory<TransactionEntity>());
            });

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                serviceProvider.GetTransactionBuilder<TransactionEntity>();

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

            Guid entityId = Guid.NewGuid();

            IServiceProvider? serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddScoped(serviceProvider =>
                    GetMockedTransactionRepositoryFactory<TransactionEntity>());
            });

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                serviceProvider.GetTransactionBuilder<TransactionEntity>();

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

            Guid entityId = Guid.NewGuid();

            IServiceProvider? serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddScoped(serviceProvider =>
                    GetMockedTransactionRepositoryFactory(
                        new IFact<TransactionEntity>[] { new VersionNumberSet<TransactionEntity>(1) }));
            });

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                serviceProvider.GetTransactionBuilder<TransactionEntity>();

            await using IEntityRepository<TransactionEntity>? entityRepository =
                await serviceProvider.CreateEntityRepository<TransactionEntity>(default!);

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

            Guid entityId = Guid.NewGuid();

            IServiceProvider? serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddScoped(serviceProvider =>
                    GetMockedTransactionRepositoryFactory<TransactionEntity>());
            });

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                serviceProvider.GetTransactionBuilder<TransactionEntity>();

            await using IEntityRepository<TransactionEntity>? entityRepository =
                await serviceProvider.CreateEntityRepository<TransactionEntity>(default!);

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

            Guid entityId = Guid.NewGuid();

            IServiceProvider? serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddScoped(serviceProvider =>
                    GetMockedTransactionRepositoryFactory<TransactionEntity>());
            });

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                serviceProvider.GetTransactionBuilder<TransactionEntity>();

            // ACT

            transactionBuilder.Create(entityId, new DoNothing());

            for (ulong i = 1; i < NumberOfVersionsToTest; i++)
            {
                transactionBuilder.Append(entityId, new DoNothing());
            }

            ITransaction<TransactionEntity>? transaction = transactionBuilder.Build(default!, default!);

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

            Guid entityId = Guid.NewGuid();

            IServiceProvider? serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddScoped(serviceProvider =>
                    GetMockedTransactionRepositoryFactory(new IFact<TransactionEntity>[]
                    {
                        new NothingDone(), new VersionNumberSet<TransactionEntity>(1)
                    }));
            });

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                serviceProvider.GetTransactionBuilder<TransactionEntity>();

            await using IEntityRepository<TransactionEntity>? entityRepository =
                await serviceProvider.CreateEntityRepository<TransactionEntity>(default!);

            // ACT

            TransactionEntity? entity = await entityRepository.Get(entityId);

            await transactionBuilder.Load(entityId, entityRepository);

            transactionBuilder.Append(entityId, new DoNothing());

            ITransaction<TransactionEntity>? transaction = transactionBuilder.Build(default!, default!);

            // ASSERT

            transaction.Commands.Length.ShouldBe(1);

            transaction.Commands[0].Command.ShouldBeEquivalentTo(new DoNothing());
        }
    }
}
