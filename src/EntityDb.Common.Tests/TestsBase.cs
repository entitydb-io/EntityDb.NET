using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit.DependencyInjection;

namespace EntityDb.Common.Tests
{
    public class TestsBase
    {
        protected readonly IServiceProvider _serviceProvider;

        public TestsBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceScope GetServiceScopeWithOverrides<TStartup>(Action<IServiceCollection> configureServices)
            where TStartup : ITestStartup, new()
        {
            var serviceCollection = new ServiceCollection();

            var startup = new TStartup();

            var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
            var testOutputHelperAccessor = _serviceProvider.GetRequiredService<ITestOutputHelperAccessor>();

            serviceCollection.AddSingleton(configuration);

            startup.ConfigureServices(serviceCollection);

            configureServices.Invoke(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            startup.Configure(loggerFactory, testOutputHelperAccessor);

            var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            return serviceScopeFactory.CreateScope();
        }

        public static ITransactionRepositoryFactory<TEntity> GetMockedTransactionRepositoryFactory<TEntity>(
            ICommand<TEntity>[]? commands = null)
        {
            if (commands == null)
            {
                commands = Array.Empty<ICommand<TEntity>>();
            }

            var transactionRepositoryMock = new Mock<ITransactionRepository<TEntity>>(MockBehavior.Strict);

            transactionRepositoryMock
                .Setup(session => session.PutTransaction(It.IsAny<ITransaction<TEntity>>()))
                .ReturnsAsync(true)
                .Verifiable();

            transactionRepositoryMock
                .Setup(repository => repository.GetCommands(It.IsAny<ICommandQuery>()))
                .ReturnsAsync(commands);

            transactionRepositoryMock
                .Setup(repository => repository.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            var transactionRepositoryFactoryMock =
                new Mock<ITransactionRepositoryFactory<TEntity>>(MockBehavior.Strict);

            transactionRepositoryFactoryMock
                .Setup(factory => factory.CreateRepository(It.IsAny<string>()))
                .ReturnsAsync(transactionRepositoryMock.Object);

            transactionRepositoryFactoryMock
                .Setup(factory => factory.Dispose());

            return transactionRepositoryFactoryMock.Object;
        }
    }
}
