using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Source;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.SnapshotTransactions
{
    public abstract class SnapshotTransactionsTestsBase<TStartup> : TestsBase
        where TStartup : ITestStartup, new()
    {
        protected SnapshotTransactionsTestsBase(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        private static async Task<ITransaction<TransactionEntity>> BuildTransaction(Guid entityId, ulong from, ulong to,
            IServiceProvider serviceProvider, IEntityRepository<TransactionEntity>? entityRepository = null)
        {
            var transactionBuilder = serviceProvider.GetRequiredService<TransactionBuilder<TransactionEntity>>();

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

            return transactionBuilder.Build(Guid.NewGuid(), new NoSource());
        }

        [Theory]
        [InlineData(10, 20)]
        public async Task
            GivenSnapshottingOnNthVersion_WhenPuttingTransactionWithNthVersion_ThenSnapshotExistsAtNthVersion(
                ulong expectedSnapshotVersion, ulong expectedCurrentVersion)
        {
            // ARRANGE

            var snapshottingStrategyMock = new Mock<ISnapshottingStrategy<TransactionEntity>>(MockBehavior.Strict);

            snapshottingStrategyMock
                .Setup(strategy =>
                    strategy.ShouldPutSnapshot(It.IsAny<TransactionEntity?>(), It.IsAny<TransactionEntity>()))
                .Returns((TransactionEntity? _, TransactionEntity nextEntity) =>
                    nextEntity.VersionNumber == expectedSnapshotVersion);

            using var serviceScope = GetServiceScopeWithOverrides<TStartup>(serviceCollection =>
            {
                serviceCollection.AddSingleton(_ => snapshottingStrategyMock.Object);
            });

            var entityId = Guid.NewGuid();

            await using var entityRepository = await serviceScope.ServiceProvider
                .GetRequiredService<IEntityRepositoryFactory<TransactionEntity>>()
                    .CreateRepository("TestWrite",
                        "TestWrite");

            var firstTransaction = await BuildTransaction(entityId, 1, expectedSnapshotVersion, serviceScope.ServiceProvider);

            var firstTransactionInserted = await entityRepository.PutTransaction(firstTransaction);

            var secondTransaction = await BuildTransaction(entityId, expectedSnapshotVersion, expectedCurrentVersion,
                serviceScope.ServiceProvider, entityRepository);

            var secondTransactionInserted = await entityRepository.PutTransaction(secondTransaction);

            // ACT

            var current = await entityRepository.GetCurrent(entityId);

            var snapshot = await entityRepository.GetSnapshotOrDefault(entityId);

            // ASSERT

            firstTransactionInserted.ShouldBeTrue();
            secondTransactionInserted.ShouldBeTrue();

            snapshot.ShouldNotBeNull();
            snapshot.VersionNumber.ShouldBe(expectedSnapshotVersion);
            current.VersionNumber.ShouldBe(expectedCurrentVersion);
        }
    }
}
