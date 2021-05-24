using EntityDb.Abstractions.Snapshots;
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
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.SnapshotTransactions
{
    public abstract class SnapshotTransactionsTestsBase : TestsBase
    {
        protected SnapshotTransactionsTestsBase(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        private static async Task<ITransaction<TransactionEntity>> BuildTransaction(Guid entityId, ulong from, ulong to, IServiceProvider serviceProvider, ITransactionRepository<TransactionEntity>? transactionRepository = null, ISnapshotRepository<TransactionEntity>? snapshotRepository = null)
        {
            var transactionBuilder = new TransactionBuilder<TransactionEntity>(new ClaimsPrincipal(), serviceProvider);

            if (transactionRepository != null)
            {
                await transactionBuilder.Load(entityId, transactionRepository, snapshotRepository);
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
        public async Task GivenCachingOnNthVersion_WhenPuttingTransactionWithNthVersion_ThenSnapshotExistsAtNthVersion(ulong expectedSnapshotVersion, ulong expectedCurrentVersion)
        {
            // ARRANGE

            var cachingStrategyMock = new Mock<ICachingStrategy<TransactionEntity>>(MockBehavior.Strict);

            cachingStrategyMock
                .Setup(strategy => strategy.ShouldCache(It.IsAny<TransactionEntity?>(), It.IsAny<TransactionEntity>()))
                .Returns((TransactionEntity? previousEntity, TransactionEntity nextEntity) => nextEntity.VersionNumber == expectedSnapshotVersion);

            var serviceProvider = GetServiceProviderWithOverrides((serviceCollection) =>
            {
                serviceCollection.AddSingleton((serviceProvider) => cachingStrategyMock.Object);
            });

            var entityId = Guid.NewGuid();

            await using var transactionRepository = await serviceProvider.CreateTransactionRepository<TransactionEntity>(new TransactionSessionOptions());

            await using var snapshotRepository = await serviceProvider.CreateSnapshotRepository<TransactionEntity>(new SnapshotSessionOptions());

            var firstTransaction = await BuildTransaction(entityId, 1, expectedSnapshotVersion, serviceProvider);

            await transactionRepository.PutTransaction(firstTransaction);

            var secondTransaction = await BuildTransaction(entityId, expectedSnapshotVersion, expectedCurrentVersion, serviceProvider, transactionRepository, snapshotRepository);

            await transactionRepository.PutTransaction(secondTransaction);

            // ACT

            var current = await serviceProvider.GetEntity(entityId, transactionRepository);

            var snapshot = await snapshotRepository.GetSnapshot(entityId);

            // ASSERT

            Assert.NotNull(snapshot);
            Assert.Equal(expectedSnapshotVersion, snapshot!.VersionNumber);
            Assert.Equal(expectedCurrentVersion, current.VersionNumber);
        }
    }
}
