using System.Diagnostics;
using System.Reflection;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Extensions;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Projections;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Snapshots.Sessions;
using EntityDb.MongoDb.Sources.Queries;
using EntityDb.MongoDb.Sources.Sessions;
using EntityDb.Redis.Extensions;
using EntityDb.Redis.Snapshots.Sessions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Shouldly;
using Xunit.Abstractions;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;
using Xunit.Sdk;
using Pointer = EntityDb.Abstractions.ValueObjects.Pointer;

namespace EntityDb.Common.Tests;

public class TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    public delegate void AddDependenciesDelegate(IServiceCollection serviceCollection);

    private static readonly SourcesAdder[] AllSourceAdders =
    {
        new("MongoDb", typeof(MongoDbQueryOptions), timeStamp => timeStamp.WithMillisecondPrecision(),
            serviceCollection =>
            {
                var databaseContainerFixture = (serviceCollection
                    .Single(descriptor => descriptor.ServiceType == typeof(DatabaseContainerFixture))
                    .ImplementationInstance as DatabaseContainerFixture)!;

                serviceCollection.AddMongoDbSources(true, true);

                serviceCollection.Configure<MongoDbQueryOptions>("Count", options =>
                {
                    options.FindOptions = new FindOptions
                    {
                        Collation = new Collation("en", numericOrdering: true),
                    };
                });

                serviceCollection.ConfigureAll<MongoDbSourceSessionOptions>(options =>
                {
                    var host = databaseContainerFixture.MongoDbContainer.Hostname;
                    var port = databaseContainerFixture.MongoDbContainer.GetMappedPublicPort(27017);

                    options.ConnectionString = new UriBuilder("mongodb://", host, port).ToString();
                    options.DatabaseName = DatabaseContainerFixture.OmniParameter;
                    options.WriteTimeout = TimeSpan.FromSeconds(1);
                });

                serviceCollection.Configure<MongoDbSourceSessionOptions>(TestSessionOptions.Write,
                    options => { options.ReadOnly = false; });

                serviceCollection.Configure<MongoDbSourceSessionOptions>(TestSessionOptions.ReadOnly, options =>
                {
                    options.ReadOnly = true;
                    options.SecondaryPreferred = false;
                });

                serviceCollection.Configure<MongoDbSourceSessionOptions>(TestSessionOptions.ReadOnlySecondaryPreferred,
                    options =>
                    {
                        options.ReadOnly = true;
                        options.SecondaryPreferred = true;
                    });
            }),
    };

    private readonly IConfiguration _configuration;
    private readonly DatabaseContainerFixture? _databaseContainerFixture;
    private readonly ITest _test;
    private readonly ITestOutputHelperAccessor _testOutputHelperAccessor;

    protected TestsBase(IServiceProvider startupServiceProvider,
        DatabaseContainerFixture? databaseContainerFixture = null)
    {
        _configuration = startupServiceProvider.GetRequiredService<IConfiguration>();
        _testOutputHelperAccessor = startupServiceProvider.GetRequiredService<ITestOutputHelperAccessor>();
        _test =
            (typeof(TestOutputHelper).GetField("test", ~BindingFlags.Public)!.GetValue(_testOutputHelperAccessor.Output)
                as ITest).ShouldNotBeNull();
        _databaseContainerFixture = databaseContainerFixture;
    }

    protected Task RunGenericTestAsync(Type[] typeArguments, object?[] invokeParameters)
    {
        var methodName = $"Generic_{new StackTrace().GetFrame(1)?.GetMethod()?.Name}";

        var methodOutput = GetType()
            .GetMethod(methodName, ~BindingFlags.Public)?
            .MakeGenericMethod(typeArguments)
            .Invoke(this, invokeParameters);

        return methodOutput
            .ShouldBeAssignableTo<Task>()
            .ShouldNotBeNull();
    }

    private static SnapshotAdder MongoDbSnapshotAdder<TSnapshot>()
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        return new SnapshotAdder($"MongoDb<{typeof(TSnapshot).Name}>", typeof(TSnapshot), serviceCollection =>
        {
            var databaseContainerFixture = (serviceCollection
                .Single(descriptor => descriptor.ServiceType == typeof(DatabaseContainerFixture))
                .ImplementationInstance as DatabaseContainerFixture)!;

            serviceCollection.AddMongoDbSnapshots<TSnapshot>(true, true);

            serviceCollection.ConfigureAll<MongoDbSnapshotSessionOptions>(options =>
            {
                var host = databaseContainerFixture.MongoDbContainer.Hostname;
                var port = databaseContainerFixture.MongoDbContainer.GetMappedPublicPort(27017);

                options.ConnectionString = new UriBuilder("mongodb://", host, port).ToString();
                options.DatabaseName = DatabaseContainerFixture.OmniParameter;
                options.CollectionName = TSnapshot.MongoDbCollectionName;
                options.WriteTimeout = TimeSpan.FromSeconds(1);
            });

            serviceCollection.Configure<MongoDbSnapshotSessionOptions>(TestSessionOptions.Write,
                options => { options.ReadOnly = false; });

            serviceCollection.Configure<MongoDbSnapshotSessionOptions>(TestSessionOptions.ReadOnly, options =>
            {
                options.ReadOnly = true;
                options.SecondaryPreferred = false;
            });

            serviceCollection.Configure<MongoDbSnapshotSessionOptions>(TestSessionOptions.ReadOnlySecondaryPreferred,
                options =>
                {
                    options.ReadOnly = true;
                    options.SecondaryPreferred = true;
                });
        });
    }

    private static SnapshotAdder RedisSnapshotAdder<TSnapshot>()
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        return new SnapshotAdder($"Redis<{typeof(TSnapshot).Name}>", typeof(TSnapshot), serviceCollection =>
        {
            var databaseContainerFixture = serviceCollection
                .Single(descriptor => descriptor.ServiceType == typeof(DatabaseContainerFixture))
                .ImplementationInstance as DatabaseContainerFixture;

            serviceCollection.AddRedisSnapshots<TSnapshot>(true);

            serviceCollection.ConfigureAll<RedisSnapshotSessionOptions>(options =>
            {
                options.ConnectionString = databaseContainerFixture!.RedisContainer.GetConnectionString();
                options.KeyNamespace = TSnapshot.RedisKeyNamespace;
            });

            serviceCollection.Configure<RedisSnapshotSessionOptions>(TestSessionOptions.Write,
                options => { options.ReadOnly = false; });

            serviceCollection.Configure<RedisSnapshotSessionOptions>(TestSessionOptions.ReadOnly, options =>
            {
                options.ReadOnly = true;
                options.SecondaryPreferred = false;
            });

            serviceCollection.Configure<RedisSnapshotSessionOptions>(TestSessionOptions.ReadOnlySecondaryPreferred,
                options =>
                {
                    options.ReadOnly = true;
                    options.SecondaryPreferred = true;
                });
        });
    }

    private static IEnumerable<SnapshotAdder> AllSnapshotAdders<TSnapshot>()
        where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
    {
        yield return RedisSnapshotAdder<TSnapshot>();
        yield return MongoDbSnapshotAdder<TSnapshot>();
    }

    private static EntityAdder GetEntityAdder<TEntity>()
        where TEntity : IEntity<TEntity>
    {
        return new EntityAdder(typeof(TEntity).Name, typeof(TEntity),
            serviceCollection => { serviceCollection.AddEntity<TEntity>(); });
    }

    private static IEnumerable<SnapshotAdder> AllEntitySnapshotAdders<TEntity>()
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        return
            from snapshotAdder in AllSnapshotAdders<TEntity>()
            let entityAdder = GetEntityAdder<TEntity>()
            select snapshotAdder with
            {
                AddDependencies = snapshotAdder.AddDependencies + entityAdder.AddDependencies + (serviceCollection =>
                {
                    serviceCollection.AddEntitySnapshotSourceSubscriber<TEntity>(TestSessionOptions.ReadOnly,
                        TestSessionOptions.Write);
                }),
            };
    }

    private static IEnumerable<EntityAdder> AllEntityAdders()
    {
        yield return GetEntityAdder<TestEntity>();
    }

    private static IEnumerable<SnapshotAdder> AllEntitySnapshotAdders()
    {
        return Enumerable.Empty<SnapshotAdder>()
            .Concat(AllEntitySnapshotAdders<TestEntity>());
    }

    private static IEnumerable<SnapshotAdder> AllProjectionAdders<TProjection>()
        where TProjection : class, IProjection<TProjection>, ISnapshotWithTestLogic<TProjection>
    {
        return AllSnapshotAdders<TProjection>()
            .Select(snapshotAdder => snapshotAdder with
                {
                    AddDependencies = snapshotAdder.AddDependencies + (serviceCollection =>
                    {
                        serviceCollection.AddProjection<TProjection>();
                        serviceCollection.AddProjectionSnapshotSourceSubscriber<TProjection>(TestSessionOptions.Write);
                    }),
                }
            );
    }

    private static IEnumerable<SnapshotAdder> AllProjectionSnapshotAdders()
    {
        return Enumerable.Empty<SnapshotAdder>()
            .Concat(AllProjectionAdders<OneToOneProjection>());
    }

    public static IEnumerable<object[]> AddSourcesAndEntity()
    {
        return from sourceAdder in AllSourceAdders
            from entityAdder in AllEntityAdders()
            select new object[] { sourceAdder, entityAdder };
    }

    public static IEnumerable<object[]> AddEntity()
    {
        return from entityAdder in AllEntityAdders()
            select new object[] { entityAdder };
    }

    public static IEnumerable<object[]> AddEntitySnapshots()
    {
        return from entitySnapshotAdder in AllEntitySnapshotAdders()
            select new object[] { entitySnapshotAdder };
    }

    public static IEnumerable<object[]> AddProjectionSnapshots()
    {
        return from projectionSnapshotAdder in AllProjectionSnapshotAdders()
            select new object[] { projectionSnapshotAdder };
    }

    public static IEnumerable<object[]> AddSourcesAndEntitySnapshots()
    {
        return from sourceAdder in AllSourceAdders
            from entitySnapshotAdder in AllEntitySnapshotAdders()
            select new object[] { sourceAdder, entitySnapshotAdder };
    }

    public static IEnumerable<object[]> AddSourcesEntitySnapshotsAndProjectionSnapshots()
    {
        return from sourceAdder in AllSourceAdders
            from entitySnapshotAdder in AllEntitySnapshotAdders()
            from projectionSnapshotAdder in AllProjectionSnapshotAdders()
            select new object[] { sourceAdder, entitySnapshotAdder, projectionSnapshotAdder };
    }

    protected IServiceScope CreateServiceScope(Action<IServiceCollection>? configureServices = null)
    {
        var serviceCollection = new ServiceCollection();

        var startup = new TStartup();

        serviceCollection.AddSingleton(_configuration);
        serviceCollection.AddSingleton(_test);
        serviceCollection.AddSingleton(_testOutputHelperAccessor);

        serviceCollection.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddXunitOutput(options => { options.IncludeScopes = true; });
            loggingBuilder.AddDebug();
            loggingBuilder.AddSimpleConsole(options => { options.IncludeScopes = true; });
        });

        serviceCollection.Configure<LoggerFilterOptions>(x => { x.MinLevel = LogLevel.Debug; });

        startup.AddServices(serviceCollection);

        if (_databaseContainerFixture != null) serviceCollection.AddSingleton(_databaseContainerFixture);

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
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ));

        loggerMock
            .Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>()))
            .Returns((LogLevel logLevel) => logLevel == LogLevel.Error);

        loggerMock
            .Setup(logger => logger.Log
            (
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(exception => exception is TException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ))
            .Verifiable();

        var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);

        loggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns(loggerMock.Object);

        loggerFactoryMock
            .Setup(factory => factory.AddProvider(It.IsAny<ILoggerProvider>()));

        return (loggerFactoryMock.Object, Verifier);

        void Verifier(Times times)
        {
            loggerMock
                .Verify
                (
                    logger => logger.Log
                    (
                        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.Is<Exception>(exception => exception is TException),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                    times
                );
        }
    }

    protected static void SetupSourceRepositoryMock(Mock<ISourceRepository> sourceRepositoryMock,
        List<Source> committedSources)
    {
    }

    protected static ISourceRepositoryFactory GetMockedSourceRepositoryFactory(
        Mock<ISourceRepository> sourceRepositoryMock, List<Source>? committedSources = null)
    {
        sourceRepositoryMock
            .Setup(repository => repository.Commit(It.IsAny<Source>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Source source, CancellationToken _) =>
            {
                committedSources?.Add(source);

                return true;
            });

        sourceRepositoryMock
            .Setup(repository => repository.Dispose());

        sourceRepositoryMock
            .Setup(repository => repository.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var sourceRepositoryFactoryMock =
            new Mock<ISourceRepositoryFactory>(MockBehavior.Strict);

        sourceRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceRepositoryMock.Object);

        sourceRepositoryFactoryMock
            .Setup(factory => factory.Dispose());

        sourceRepositoryFactoryMock
            .Setup(factory => factory.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        return sourceRepositoryFactoryMock.Object;
    }

    protected static ISourceRepositoryFactory GetMockedSourceRepositoryFactory(
        object[]? deltas = null)
    {
        deltas ??= Array.Empty<object>();

        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        sourceRepositoryMock
            .Setup(repository =>
                repository.Commit(It.IsAny<Source>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateDeltas(It.IsAny<IMessageQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerablePolyfill.FromResult(deltas));

        return GetMockedSourceRepositoryFactory(sourceRepositoryMock, new List<Source>());
    }

    protected static ISnapshotRepositoryFactory<TEntity> GetMockedSnapshotRepositoryFactory<TEntity>
    (
        TEntity? snapshot = default
    )
    {
        var snapshotRepositoryMock = new Mock<ISnapshotRepository<TEntity>>(MockBehavior.Strict);

        snapshotRepositoryMock
            .Setup(repository => repository.GetSnapshotOrDefault(It.IsAny<Pointer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        snapshotRepositoryMock
            .Setup(repository => repository.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var snapshotRepositoryFactoryMock = new Mock<ISnapshotRepositoryFactory<TEntity>>(MockBehavior.Strict);

        snapshotRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshotRepositoryMock.Object);

        snapshotRepositoryFactoryMock
            .Setup(factory => factory.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        return snapshotRepositoryFactoryMock.Object;
    }

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

    public record SourcesAdder(string Name, Type QueryOptionsType, Func<TimeStamp, TimeStamp> FixTimeStamp,
        AddDependenciesDelegate AddDependencies)
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

    public record EntityAdder(string Name, Type EntityType, AddDependenciesDelegate AddDependencies)
    {
        public override string ToString()
        {
            return Name;
        }
    }
}