using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.AgentSignature;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.Entities
{
    public class EntityTests : TestsBase<Startup>
    {
        public EntityTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        private static async Task<ITransaction<TransactionEntity>> BuildTransaction
        (
            IServiceScope serviceScope,
            Guid entityId,
            ulong from,
            ulong to,
            IEntityRepository<TransactionEntity>? entityRepository = null
        )
        {
            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            if (entityRepository != null)
            {
                await transactionBuilder.Load(entityId, entityRepository);
            }
            else
            {
                transactionBuilder.Create(entityId, new DoNothing());
            }

            for (var i = from; i < to; i++)
            {
                transactionBuilder.Append(entityId, new DoNothing());
            }

            return transactionBuilder.Build(Guid.NewGuid());
        }

        [Fact]
        public async Task GivenExistingEntityWithNoSnapshotting_WhenGettingEntity_ThenGetCommandsRuns()
        {
            // ARRANGE

            const ulong ExpectedVersionNumber = 10;

            var entityId = Guid.NewGuid();

            var commands = new List<ICommand<TransactionEntity>>();

            var transactionRepositoryMock = new Mock<ITransactionRepository<TransactionEntity>>(MockBehavior.Strict);

            transactionRepositoryMock
                .Setup(repository => repository.PutTransaction(It.IsAny<ITransaction<TransactionEntity>>()))
                .ReturnsAsync((ITransaction<TransactionEntity> transaction) =>
                {
                    commands.AddRange(transaction.Steps.Select(step => step.Command));

                    return true;
                });

            transactionRepositoryMock
                .Setup(factory => factory.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            transactionRepositoryMock
                .Setup(repository => repository.GetCommands(It.IsAny<ICommandQuery>()))
                .ReturnsAsync(() =>
                {
                    return commands.ToArray();
                })
                .Verifiable();

            var transactionRepositoryFactoryMock =
                new Mock<ITransactionRepositoryFactory<TransactionEntity>>(MockBehavior.Strict);

            transactionRepositoryFactoryMock
                .Setup(factory => factory.CreateRepository(It.IsAny<string>()))
                .ReturnsAsync(transactionRepositoryMock.Object);

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                serviceCollection.AddSingleton(transactionRepositoryFactoryMock.Object);
            });

            await using var entityRepository = await serviceScope.ServiceProvider
                .GetRequiredService<IEntityRepositoryFactory<TransactionEntity>>()
                    .CreateRepository(TestSessionOptions.Write);

            var transaction = await BuildTransaction(serviceScope, entityId, 1, ExpectedVersionNumber);

            var transactionInserted = await entityRepository.PutTransaction(transaction);

            // ARRANGE ASSERTIONS

            transactionInserted.ShouldBeTrue();

            // ACT

            var currenEntity = await entityRepository.GetCurrent(entityId);

            // ASSERT

            currenEntity.VersionNumber.ShouldBe(ExpectedVersionNumber);

            transactionRepositoryMock
                .Verify(repository => repository.GetCommands(It.IsAny<ICommandQuery>()), Times.Once);
        }
    }
}
