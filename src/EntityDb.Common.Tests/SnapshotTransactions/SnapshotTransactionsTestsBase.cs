using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
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
    public abstract class SnapshotTransactionsTestsBase : TestsBase
    {
        protected SnapshotTransactionsTestsBase(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        private static async Task<ITransaction<TransactionEntity>> BuildTransaction(Guid entityId, ulong from, ulong to,
            IServiceProvider serviceProvider, IEntityRepository<TransactionEntity>? entityRepository = null)
        {
            var transactionBuilder = serviceProvider.GetTransactionBuilder<TransactionEntity>();

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
        public async Task GivenCachingOnNthVersion_WhenPuttingTransactionWithNthVersion_ThenSnapshotExistsAtNthVersion(
            ulong expectedSnapshotVersion, ulong expectedCurrentVersion)
        {
            // ARRANGE

            var cachingStrategyMock = new Mock<ISnapshottingStrategy<TransactionEntity>>(MockBehavior.Strict);

            cachingStrategyMock
                .Setup(strategy =>
                    strategy.ShouldPutSnapshot(It.IsAny<TransactionEntity?>(), It.IsAny<TransactionEntity>()))
                .Returns((TransactionEntity? _, TransactionEntity nextEntity) =>
                    nextEntity.VersionNumber == expectedSnapshotVersion);

            var serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddSingleton(_ => cachingStrategyMock.Object);
            });

            var entityId = Guid.NewGuid();

            await using var entityRepository =
                await serviceProvider.CreateEntityRepository<TransactionEntity>(new TransactionSessionOptions(),
                    new SnapshotSessionOptions());

            var firstTransaction = await BuildTransaction(entityId, 1, expectedSnapshotVersion, serviceProvider);

            await entityRepository.PutTransaction(firstTransaction);

            var secondTransaction = await BuildTransaction(entityId, expectedSnapshotVersion, expectedCurrentVersion,
                serviceProvider, entityRepository);

            await entityRepository.PutTransaction(secondTransaction);

            // ACT

            var current = await entityRepository.GetCurrentOrConstruct(entityId);

            var snapshot = await entityRepository.GetSnapshotOrDefault(entityId);

            // ASSERT

            snapshot.ShouldNotBeNull();
            snapshot.VersionNumber.ShouldBe(expectedSnapshotVersion);
            current.VersionNumber.ShouldBe(expectedCurrentVersion);
        }
    }
}
