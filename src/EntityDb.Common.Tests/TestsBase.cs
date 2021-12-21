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
using Xunit.DependencyInjection.Logging;

namespace EntityDb.Common.Tests
{
    public class TestsBase<TStartup>
        where TStartup : IStartup, new()
    {
        public record TestServiceScope(ServiceProvider SingletonServiceProvider, IServiceScope ServiceScope) : IServiceScope, IDisposable
        {
            public IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

            public void Dispose()
            {
                ServiceScope.Dispose();

                SingletonServiceProvider.Dispose();
            }
        }

        private readonly IConfiguration _configuration;
        private readonly ITestOutputHelperAccessor? _testOutputHelperAccessor;

        public TestsBase(IServiceProvider startupServiceProvider)
        {
            _configuration = startupServiceProvider.GetRequiredService<IConfiguration>();
            _testOutputHelperAccessor = startupServiceProvider.GetService<ITestOutputHelperAccessor>();
        }

        public TestServiceScope CreateServiceScope(Action<IServiceCollection>? configureServices = null)
        {
            var serviceCollection = new ServiceCollection();

            var startup = new TStartup();

            serviceCollection.AddSingleton(_configuration);

            startup.AddServices(serviceCollection);

            configureServices?.Invoke(serviceCollection);

            var singletonServiceProvider = serviceCollection.BuildServiceProvider();

            if (_testOutputHelperAccessor != null)
            {
                var loggerFactory = singletonServiceProvider.GetRequiredService<ILoggerFactory>();

                loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(_testOutputHelperAccessor));
            }

            var serviceScopeFactory = singletonServiceProvider.GetRequiredService<IServiceScopeFactory>();

            return new(singletonServiceProvider, serviceScopeFactory.CreateScope());
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
