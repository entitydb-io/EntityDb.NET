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
using EntityDb.Common.Extensions;
using EntityDb.Common.Tests.Implementations.Projections;
using EntityDb.InMemory.Extensions;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.Redis.Extensions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;
using Xunit.Sdk;
using Shouldly;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.Common.Entities;
using EntityDb.Common.Projections;

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

    public delegate void AddDependenciesDelegate(IServiceCollection serviceCollection);

    public record TransactionsAdder(string Name, AddDependenciesDelegate AddDependencies)
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public record SnapshotAdder(string Name, Type SnapshotType, AddDependenciesDelegate AddDependencies)
    {
        public override string ToString()
        {
            return Name;
        }
    }

    private readonly IConfiguration _configuration;
    private readonly ITestOutputHelperAccessor _testOutputHelperAccessor;
    private readonly ITest _test;

    protected Task RunGenericTestAsync(string methodName, Type[] typeArguments, object?[] invokeParameters)
    {
        var expectedMethodName = $"Generic_{new StackTrace().GetFrame(1)?.GetMethod()?.Name}";

        methodName.ShouldBe(expectedMethodName);

        var testTask = GetType()
            .GetMethod(methodName, ~BindingFlags.Public)?
            .MakeGenericMethod(typeArguments)
            .Invoke(this, invokeParameters);

        return testTask
            .ShouldBeAssignableTo<Task>()
            .ShouldNotBeNull();
    }

    protected TestsBase(IServiceProvider startupServiceProvider)
    {
        _configuration = startupServiceProvider.GetRequiredService<IConfiguration>();
        _testOutputHelperAccessor = startupServiceProvider.GetRequiredService<ITestOutputHelperAccessor>();
        _test = (typeof(TestOutputHelper).GetField("test", ~BindingFlags.Public)!.GetValue(_testOutputHelperAccessor.Output) as ITest).ShouldNotBeNull();
    }

    private static readonly TransactionsAdder[] AllTransactionAdders =
    {
        new("MongoDb", serviceCollection =>
        {
            serviceCollection.AddAutoProvisionMongoDbTransactions
            (
                "Test",
                _ => "mongodb://127.0.0.1:27017/?connect=direct&replicaSet=entitydb",
                true
            );
        }),
    };

    private static SnapshotAdder RedisSnapshotAdder<TSnapshot>()
        where TSnapshot : ISnapshotWithTestLogic<TSnapshot>
    {
        return new($"Redis<{typeof(TSnapshot).Name}>", typeof(TSnapshot), serviceCollection =>
        {
            serviceCollection.AddRedisSnapshots<TSnapshot>
            (
                TSnapshot.RedisKeyNamespace,
                _ => "127.0.0.1:6379",
                true
            );
        });
    }

    private static SnapshotAdder InMemorySnapshotAdder<TSnapshot>()
        where TSnapshot : ISnapshotWithTestLogic<TSnapshot>
    {
        return new($"InMemory<{typeof(TSnapshot).Name}>", typeof(TSnapshot), serviceCollection =>
        {
            serviceCollection.AddInMemorySnapshots<TSnapshot>
            (
                testMode: true
            );
        });
    }

    private static SnapshotAdder[] AllSnapshotAdders<TSnapshot>()
        where TSnapshot : ISnapshotWithTestLogic<TSnapshot>
    {
        return new[]
        {
            RedisSnapshotAdder<TSnapshot>(),
            InMemorySnapshotAdder<TSnapshot>()
        };
    }

    private static IEnumerable<SnapshotAdder> AllEntitySnapshotAdders<TEntity>()
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        foreach (var snapshotAdder in AllSnapshotAdders<TEntity>())
        {
            yield return new(snapshotAdder.Name, snapshotAdder.SnapshotType, snapshotAdder.AddDependencies + (serviceCollection =>
            {
                serviceCollection.AddEntity<TEntity>();

                serviceCollection.AddEntitySnapshotTransactionSubscriber<TEntity>(TestSessionOptions.ReadOnly, TestSessionOptions.Write, true);
            }));
        }
    }

    private static IEnumerable<SnapshotAdder> AllEntitySnapshotAdders()
    {
        return Enumerable.Empty<SnapshotAdder>()
            .Concat(AllEntitySnapshotAdders<TestEntity>());
    }

    private static IEnumerable<SnapshotAdder> AllProjectionAdders<TProjection>()
        where TProjection : IProjection<TProjection>, ISnapshotWithTestLogic<TProjection>
    {
        foreach (var snapshotAdder in AllSnapshotAdders<TProjection>())
        {
            yield return new(snapshotAdder.Name, snapshotAdder.SnapshotType, snapshotAdder.AddDependencies + (serviceCollection =>
            {
                serviceCollection.AddProjection<TProjection>();
                serviceCollection.AddProjectionSnapshotTransactionSubscriber<TProjection>(TestSessionOptions.ReadOnly, TestSessionOptions.Write, true);
            }));
        }
    }

    private static IEnumerable<SnapshotAdder> AllProjectionSnapshotAdders()
    {
        return Enumerable.Empty<SnapshotAdder>()
            .Concat(AllProjectionAdders<OneToOneProjection>());
    }

    public static IEnumerable<object[]> AddTransactions() =>
        from transactionAdder in AllTransactionAdders
        select new object[] { transactionAdder };

    public static IEnumerable<object[]> AddEntitySnapshots() =>
        from entitySnapshotAdder in AllEntitySnapshotAdders()
        select new object[] { entitySnapshotAdder };

    public static IEnumerable<object[]> AddProjectionSnapshots() =>
        from projectionSnapshotAdder in AllProjectionSnapshotAdders()
        select new object[] { projectionSnapshotAdder };

    public static IEnumerable<object[]> AddTransactionsAndEntitySnapshots() =>
        from transactionAdder in AllTransactionAdders
        from entitySnapshotAdder in AllEntitySnapshotAdders()
        select new object[] { transactionAdder, entitySnapshotAdder };

    public static IEnumerable<object[]> AddTransactionsEntitySnapshotsAndProjectionSnapshots() =>
        from transactionAdder in AllTransactionAdders
        from entitySnapshotAdder in AllEntitySnapshotAdders()
        from projectionSnapshotAdder in AllProjectionSnapshotAdders()
        select new object[] { transactionAdder, entitySnapshotAdder, projectionSnapshotAdder };

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
            .Setup(repository => repository.GetSnapshotOrDefault(It.IsAny<Abstractions.ValueObjects.Pointer>(), It.IsAny<CancellationToken>()))
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