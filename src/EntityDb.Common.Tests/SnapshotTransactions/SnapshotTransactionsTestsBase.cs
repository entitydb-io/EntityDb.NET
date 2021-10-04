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
            TransactionBuilder<TransactionEntity>? transactionBuilder =
                serviceProvider.GetTransactionBuilder<TransactionEntity>();

            if (entityRepository != null)
            {
                await transactionBuilder.Load(entityId, entityRepository);
            }
            else
            {
                transactionBuilder.Create(entityId, new DoNothing());
            }

            for (ulong i = from; i < to; i++)
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

            Mock<ISnapshottingStrategy<TransactionEntity>>? cachingStrategyMock = new(MockBehavior.Strict);

            cachingStrategyMock
                .Setup(strategy =>
                    strategy.ShouldPutSnapshot(It.IsAny<TransactionEntity?>(), It.IsAny<TransactionEntity>()))
                .Returns((TransactionEntity? previousEntity, TransactionEntity nextEntity) =>
                    nextEntity.VersionNumber == expectedSnapshotVersion);

            IServiceProvider? serviceProvider = GetServiceProviderWithOverrides(serviceCollection =>
            {
                serviceCollection.AddSingleton(serviceProvider => cachingStrategyMock.Object);
            });

            Guid entityId = Guid.NewGuid();

            await using IEntityRepository<TransactionEntity>? entityRepository =
                await serviceProvider.CreateEntityRepository<TransactionEntity>(new TransactionSessionOptions(),
                    new SnapshotSessionOptions());

            ITransaction<TransactionEntity>? firstTransaction =
                await BuildTransaction(entityId, 1, expectedSnapshotVersion, serviceProvider);

            await entityRepository.Put(firstTransaction);

            ITransaction<TransactionEntity>? secondTransaction = await BuildTransaction(entityId,
                expectedSnapshotVersion, expectedCurrentVersion, serviceProvider, entityRepository);

            await entityRepository.Put(secondTransaction);

            // ACT

            TransactionEntity? current = await entityRepository.Get(entityId);

            TransactionEntity? snapshot = await entityRepository.SnapshotRepository!.GetSnapshot(entityId);

            // ASSERT

            snapshot.ShouldNotBeNull();
            snapshot.VersionNumber.ShouldBe(expectedSnapshotVersion);
            current.VersionNumber.ShouldBe(expectedCurrentVersion);
        }
    }
}
