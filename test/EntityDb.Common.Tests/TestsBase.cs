using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Tests.Implementations.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.InMemory.Extensions;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.Redis.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace EntityDb.Common.Tests;

public class TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    private record TestServiceScope
        (ServiceProvider SingletonServiceProvider, IServiceScope ServiceScope) : IServiceScope
    {
        public IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

        public void Dispose()
        {
            ServiceScope.Dispose();

            SingletonServiceProvider.Dispose();
        }
    }

    public delegate void AddTransactionsDelegate(IServiceCollection serviceCollection);

    public delegate void AddSnapshotsDelegate(IServiceCollection serviceCollection);

    private readonly IConfiguration _configuration;
    private readonly ITestOutputHelperAccessor? _testOutputHelperAccessor;

    protected TestsBase(IServiceProvider startupServiceProvider)
    {
        _configuration = startupServiceProvider.GetRequiredService<IConfiguration>();
        _testOutputHelperAccessor = startupServiceProvider.GetService<ITestOutputHelperAccessor>();
    }

    
    private static readonly AddTransactionsDelegate[] AllAddTransactionsDelegates =
    {
        serviceCollection =>
        {
            serviceCollection.AddAutoProvisionMongoDbTransactions
            (
                TransactionEntity.MongoCollectionName,
                _ => "mongodb://127.0.0.1:27017/?connect=direct&replicaSet=entitydb",
                true
            );
        }
    };

    private static readonly AddSnapshotsDelegate[] AllAddSnapshotsDelegates =
    {
        serviceCollection =>
        {
            serviceCollection.AddRedisSnapshots<TransactionEntity>
            (
                TransactionEntity.RedisKeyNamespace,
                _ => "127.0.0.1:6379",
                true
            );
        },
        serviceCollection =>
        {
            serviceCollection.AddInMemorySnapshots<TransactionEntity>
            (
                testMode: true
            );
        }
    };

    public static IEnumerable<object[]> AddTransactionsAndSnapshots() =>
        from addTransactionsDelegate in AllAddTransactionsDelegates
        from addSnapshotsDelegate in AllAddSnapshotsDelegates
        select new object[] { addTransactionsDelegate, addSnapshotsDelegate };

    public static IEnumerable<object[]> AddTransactions() => AllAddTransactionsDelegates
        .Select(addTransactionsDelegate => new object[] { addTransactionsDelegate });

    public static IEnumerable<object[]> AddSnapshots() => AllAddSnapshotsDelegates
        .Select(addSnapshotsDelegate => new object[] { addSnapshotsDelegate });
    
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
            loggerFactory.AddProvider(new DebugLoggerProvider());
        }

        var serviceScopeFactory = singletonServiceProvider.GetRequiredService<IServiceScopeFactory>();

        return new TestServiceScope(singletonServiceProvider, serviceScopeFactory.CreateScope());
    }

    protected static (ILoggerFactory Logger, Action<Times> LoggerVerifier) GetMockedLoggerFactory<TException>()
        where TException : Exception
    {
        var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

        loggerMock
            .Setup(logger => logger.Log
            (
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<TException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ));
        
        loggerMock
            .Setup(logger => logger.Log
            (
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<TException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ))
            .Verifiable();

        var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);

        loggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns(loggerMock.Object);

        loggerFactoryMock
            .Setup(factory => factory.AddProvider(It.IsAny<ILoggerProvider>()));

        void Verifier(Times times)
        {
            loggerMock
                .Verify
                (
                    logger => logger.Log
                    (
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<TException>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                    times
                );
        }

        return (loggerFactoryMock.Object, Verifier);
    }

    protected static ITransactionRepositoryFactory GetMockedTransactionRepositoryFactory(
        object[]? commands = null)
    {
        commands ??= Array.Empty<object>();

        var transactionRepositoryMock = new Mock<ITransactionRepository>(MockBehavior.Strict);

        transactionRepositoryMock
            .Setup(repository => repository.PutTransaction(It.IsAny<ITransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        transactionRepositoryMock
            .Setup(repository => repository.GetCommands(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commands);

        transactionRepositoryMock
            .Setup(repository => repository.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var transactionRepositoryFactoryMock =
            new Mock<ITransactionRepositoryFactory>(MockBehavior.Strict);

        transactionRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
            .Setup(repository => repository.GetSnapshot(It.IsAny<Id>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        snapshotRepositoryMock
            .Setup(repository => repository.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var snapshotRepositoryFactoryMock = new Mock<ISnapshotRepositoryFactory<TransactionEntity>>(MockBehavior.Strict);

        snapshotRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshotRepositoryMock.Object);

        snapshotRepositoryFactoryMock
            .Setup(factory => factory.Dispose());

        return snapshotRepositoryFactoryMock.Object;
    }
}