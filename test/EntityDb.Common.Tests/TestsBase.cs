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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Extensions;
using EntityDb.Common.Tests.Implementations.Projections;
using EntityDb.InMemory.Extensions;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.Redis.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;
using Xunit.Sdk;

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

    public record TransactionsAdder(string Name, Type EntityType, AddTransactionsDelegate AddTransactionsDelegate)
    {
        public void Add(IServiceCollection serviceCollection)
        {
            AddTransactionsDelegate.Invoke(serviceCollection);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public delegate void AddSnapshotsDelegate(IServiceCollection serviceCollection);

    public record SnapshotsAdder(string Name, Type SnapshotType, AddSnapshotsDelegate AddSnapshotsDelegate)
    {
        public void Add(IServiceCollection serviceCollection)
        {
            AddSnapshotsDelegate.Invoke(serviceCollection);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    private readonly IConfiguration _configuration;
    private readonly ITestOutputHelperAccessor _testOutputHelperAccessor;
    private readonly ITest _test;

    protected TestsBase(IServiceProvider startupServiceProvider)
    {
        _configuration = startupServiceProvider.GetRequiredService<IConfiguration>();
        _testOutputHelperAccessor = startupServiceProvider.GetRequiredService<ITestOutputHelperAccessor>();
        _test = (typeof(TestOutputHelper).GetField("test", ~BindingFlags.Public)!.GetValue(_testOutputHelperAccessor.Output) as ITest)!;
    }

    private static readonly TransactionsAdder[] AllTransactionsAdders =
    {
        new("MongoDb<TestEntity>", typeof(TestEntity), serviceCollection =>
        {
            serviceCollection.AddAutoProvisionMongoDbTransactions
            (
                TestEntity.MongoCollectionName,
                _ => "mongodb://127.0.0.1:27017/?connect=direct&replicaSet=entitydb",
                true
            );
        })
    };

    private static readonly AddSnapshotsDelegate AddEntitySnapshotsSharedResources = serviceCollection =>
    {
        serviceCollection.AddEntitySnapshotTransactionSubscriber<TestEntity>(TestSessionOptions.Write, true);
    };

    private static readonly SnapshotsAdder[] AllEntitySnapshotsAdders =
    {
        new("Redis<TestEntity>", typeof(TestEntity), AddEntitySnapshotsSharedResources + (serviceCollection =>
        {
            serviceCollection.AddRedisSnapshots<TestEntity>
            (
                TestEntity.RedisKeyNamespace,
                _ => "127.0.0.1:6379",
                true
            );
        })),
        new("InMemory<TestEntity>", typeof(TestEntity), AddEntitySnapshotsSharedResources + (serviceCollection =>
        {
            serviceCollection.AddInMemorySnapshots<TestEntity>
            (
                testMode: true
            );
        }))
    };

    private static readonly AddSnapshotsDelegate AddOneToOneProjectionSnapshotsSharedResources = serviceCollection =>
    {
        serviceCollection.AddProjection<OneToOneProjection, SingleEntityProjectionStrategy>();
        serviceCollection.AddProjectionSnapshotTransactionSubscriber<OneToOneProjection>(TestSessionOptions.Write, true);
    };

    private static readonly SnapshotsAdder[] AllOneToOneProjectionSnapshotsAdders =
    {
        new("Redis<OneToOneProjection>", typeof(OneToOneProjection), AddOneToOneProjectionSnapshotsSharedResources + (serviceCollection =>
        {
            serviceCollection.AddRedisSnapshots<OneToOneProjection>
            (
                OneToOneProjection.RedisKeyNamespace,
                _ => "127.0.0.1:6379",
                true
            );
        })),
        new("InMemory<OneToOneProjection>", typeof(OneToOneProjection), AddOneToOneProjectionSnapshotsSharedResources + (serviceCollection =>
        {
            serviceCollection.AddInMemorySnapshots<OneToOneProjection>
            (
                testMode: true
            );
        }))
    };

    public static IEnumerable<object[]> AddTransactionsAndEntitySnapshots() =>
        from transactionsAdder in AllTransactionsAdders
        from snapshotsAdder in AllEntitySnapshotsAdders
        select new object[] { transactionsAdder, snapshotsAdder };
    
    public static IEnumerable<object[]> AddTransactionsAndOneToOneProjectionSnapshots() =>
        from transactionsAdder in AllTransactionsAdders
        from snapshotsAdder in AllOneToOneProjectionSnapshotsAdders
        select new object[] { transactionsAdder, snapshotsAdder };

    public static IEnumerable<object[]> AddTransactions() => AllTransactionsAdders
        .Select(transactionsAdder => new object[] { transactionsAdder });

    public static IEnumerable<object[]> AddEntitySnapshots() => AllEntitySnapshotsAdders
        .Select(snapshotsAdder => new object[] { snapshotsAdder });

    public static IEnumerable<object[]> AddOneToOneProjectionSnapshots() => AllOneToOneProjectionSnapshotsAdders
        .Select(snapshotsAdder => new object[] { snapshotsAdder });

    protected IServiceScope CreateServiceScope(Action<IServiceCollection>? configureServices = null)
    {
        var serviceCollection = new ServiceCollection();

        var startup = new TStartup();

        serviceCollection.AddSingleton(_configuration);
        serviceCollection.AddSingleton(_test);

        serviceCollection.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddProvider(new XunitTestOutputLoggerProvider(_testOutputHelperAccessor));
            loggingBuilder.AddDebug();
            loggingBuilder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
            });
        });

        startup.AddServices(serviceCollection);

        configureServices?.Invoke(serviceCollection);
        
        serviceCollection.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
        
        var singletonServiceProvider = serviceCollection.BuildServiceProvider();

        var serviceScopeFactory = singletonServiceProvider.GetRequiredService<IServiceScopeFactory>();

        return new TestServiceScope(singletonServiceProvider, serviceScopeFactory.CreateScope());
    }

    protected static (ILoggerFactory Logger, Action<Times> LoggerVerifier) GetMockedLoggerFactory<TException>()
        where TException : Exception
    {
        var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

        disposableMock.Setup(disposable => disposable.Dispose());
        
        var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

        loggerMock
            .Setup(logger => logger.BeginScope(It.IsAny<It.IsAnyType>()))
            .Returns(disposableMock.Object);
        
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

    protected static ISnapshotRepositoryFactory<TestEntity> GetMockedSnapshotRepositoryFactory
    (
        TestEntity? snapshot = null
    )
    {
        var snapshotRepositoryMock = new Mock<ISnapshotRepository<TestEntity>>(MockBehavior.Strict);

        snapshotRepositoryMock
            .Setup(repository => repository.GetSnapshot(It.IsAny<Id>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        snapshotRepositoryMock
            .Setup(repository => repository.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var snapshotRepositoryFactoryMock = new Mock<ISnapshotRepositoryFactory<TestEntity>>(MockBehavior.Strict);

        snapshotRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshotRepositoryMock.Object);

        snapshotRepositoryFactoryMock
            .Setup(factory => factory.Dispose());

        return snapshotRepositoryFactoryMock.Object;
    }
}