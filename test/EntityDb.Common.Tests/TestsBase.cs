using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Tests.Implementations.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace EntityDb.Common.Tests;

public class TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    private record TestServiceScope(ServiceProvider SingletonServiceProvider, IServiceScope ServiceScope) : IServiceScope
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

    protected TestsBase(IServiceProvider startupServiceProvider)
    {
        _configuration = startupServiceProvider.GetRequiredService<IConfiguration>();
        _testOutputHelperAccessor = startupServiceProvider.GetService<ITestOutputHelperAccessor>();
    }

    protected IServiceScope CreateServiceScope(Action<IServiceCollection>? configureServices = null)
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

        return new TestServiceScope(singletonServiceProvider, serviceScopeFactory.CreateScope());
    }

    protected static ITransactionRepositoryFactory GetMockedTransactionRepositoryFactory(
        object[]? commands = null)
    {
        commands ??= Array.Empty<object>();

        var transactionRepositoryMock = new Mock<ITransactionRepository>(MockBehavior.Strict);

        transactionRepositoryMock
            .Setup(repository => repository.PutTransaction(It.IsAny<ITransaction>()))
            .ReturnsAsync(true);

        transactionRepositoryMock
            .Setup(repository => repository.GetCommands(It.IsAny<ICommandQuery>()))
            .ReturnsAsync(commands);

        transactionRepositoryMock
            .Setup(repository => repository.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var transactionRepositoryFactoryMock =
            new Mock<ITransactionRepositoryFactory>(MockBehavior.Strict);

        transactionRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>()))
            .ReturnsAsync(transactionRepositoryMock.Object);

        transactionRepositoryFactoryMock
            .Setup(factory => factory.Dispose());

        return transactionRepositoryFactoryMock.Object;
    }

    protected static ISnapshotRepositoryFactory<TransactionEntity> GetMockedSnapshotRepositoryFactory
    (
        TransactionEntity? snapshot = null
    )
    {
        var snapshotRepositoryMock = new Mock<ISnapshotRepository<TransactionEntity>>(MockBehavior.Strict);

        snapshotRepositoryMock
            .Setup(repository => repository.GetSnapshot(It.IsAny<Id>()))
            .ReturnsAsync(snapshot);

        snapshotRepositoryMock
            .Setup(repository => repository.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var snapshotRepositoryFactoryMock = new Mock<ISnapshotRepositoryFactory<TransactionEntity>>(MockBehavior.Strict);

        snapshotRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>()))
            .ReturnsAsync(snapshotRepositoryMock.Object);

        snapshotRepositoryFactoryMock
            .Setup(factory => factory.Dispose());

        return snapshotRepositoryFactoryMock.Object;
    }
}