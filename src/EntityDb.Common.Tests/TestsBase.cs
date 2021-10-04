using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Tests
{
    public class TestsBase
    {
        protected readonly IServiceProvider _serviceProvider;

        public TestsBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceProvider GetEmptyServiceProvider()
        {
            return new ServiceCollection().BuildServiceProvider();
        }

        public IServiceProvider GetServiceProviderWithOverrides(Action<IServiceCollection> configureOverrides)
        {
            var overrideServiceCollection = new ServiceCollection();

            configureOverrides.Invoke(overrideServiceCollection);

            return new ServiceProviderWithOverrides(overrideServiceCollection.BuildServiceProvider(), _serviceProvider);
        }

        public static ITransactionRepositoryFactory<TEntity> GetMockedTransactionRepositoryFactory<TEntity>(
            IFact<TEntity>[]? facts = null)
        {
            if (facts == null)
            {
                facts = Array.Empty<IFact<TEntity>>();
            }

            var transactionRepositoryMock = new Mock<ITransactionRepository<TEntity>>(MockBehavior.Strict);

            transactionRepositoryMock
                .Setup(session => session.PutTransaction(It.IsAny<ITransaction<TEntity>>()))
                .ReturnsAsync(true)
                .Verifiable();

            transactionRepositoryMock
                .Setup(repository => repository.GetFacts(It.IsAny<IFactQuery>()))
                .ReturnsAsync(facts);

            transactionRepositoryMock
                .Setup(repository => repository.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            var transactionRepositoryFactoryMock =
                new Mock<ITransactionRepositoryFactory<TEntity>>(MockBehavior.Strict);

            transactionRepositoryFactoryMock
                .Setup(factory => factory.CreateRepository(It.IsAny<ITransactionSessionOptions>()))
                .ReturnsAsync(transactionRepositoryMock.Object);

            return transactionRepositoryFactoryMock.Object;
        }

        private record ServiceProviderWithOverrides(IServiceProvider OverrideServiceProvider,
            IServiceProvider ServiceProvider) : IServiceProvider
        {
            public object? GetService(Type serviceType)
            {
                return OverrideServiceProvider.GetService(serviceType) ?? ServiceProvider.GetService(serviceType);
            }
        }
    }
}
