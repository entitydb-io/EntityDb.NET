using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.Streams;
using EntityDb.Common.Disposables;
using EntityDb.Common.Extensions;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Projections;
using EntityDb.Common.Tests.Implementations.States;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Sources.Queries;
using EntityDb.MongoDb.Sources.Sessions;
using EntityDb.MongoDb.States.Sessions;
using EntityDb.Redis.Extensions;
using EntityDb.Redis.States.Sessions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Shouldly;
using System.Diagnostics;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;
using Xunit.Sdk;

namespace EntityDb.Common.Tests;

public abstract class TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    public delegate void AddDependenciesDelegate(IServiceCollection serviceCollection);

    private static readonly SourceRepositoryAdder[] AllSourceRepositoryAdders =
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
                    options.FindOptions = new FindOptions { Collation = new Collation("en", numericOrdering: true) };
                });

                serviceCollection.ConfigureAll<MongoDbSourceSessionOptions>(options =>
                {
                    options.ConnectionString = databaseContainerFixture.MongoDbContainer.GetConnectionString();
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

    private static StateRepositoryAdder MongoDbStateRepositoryAdder<TState>()
        where TState : class, IStateWithTestLogic<TState>
    {
        return new StateRepositoryAdder($"MongoDb<{typeof(TState).Name}>", typeof(TState), serviceCollection =>
        {
            var databaseContainerFixture = (serviceCollection
                .Single(descriptor => descriptor.ServiceType == typeof(DatabaseContainerFixture))
                .ImplementationInstance as DatabaseContainerFixture)!;

            serviceCollection.AddMongoDbStateRepository<TState>(true, true);

            serviceCollection.ConfigureAll<MongoDbStateSessionOptions>(options =>
            {
                options.ConnectionString = databaseContainerFixture.MongoDbContainer.GetConnectionString();
                options.DatabaseName = DatabaseContainerFixture.OmniParameter;
                options.CollectionName = TState.MongoDbCollectionName;
                options.WriteTimeout = TimeSpan.FromSeconds(1);
            });

            serviceCollection.Configure<MongoDbStateSessionOptions>(TestSessionOptions.Write,
                options => { options.ReadOnly = false; });

            serviceCollection.Configure<MongoDbStateSessionOptions>(TestSessionOptions.ReadOnly, options =>
            {
                options.ReadOnly = true;
                options.SecondaryPreferred = false;
            });

            serviceCollection.Configure<MongoDbStateSessionOptions>(TestSessionOptions.ReadOnlySecondaryPreferred,
                options =>
                {
                    options.ReadOnly = true;
                    options.SecondaryPreferred = true;
                });
        });
    }

    private static StateRepositoryAdder RedisStateRepositoryAdder<TState>()
        where TState : class, IStateWithTestLogic<TState>
    {
        return new StateRepositoryAdder($"Redis<{typeof(TState).Name}>", typeof(TState), serviceCollection =>
        {
            var databaseContainerFixture = serviceCollection
                .Single(descriptor => descriptor.ServiceType == typeof(DatabaseContainerFixture))
                .ImplementationInstance as DatabaseContainerFixture;

            serviceCollection.AddRedisStateRepository<TState>(true);

            serviceCollection.ConfigureAll<RedisStateSessionOptions>(options =>
            {
                options.ConnectionString = databaseContainerFixture!.RedisContainer.GetConnectionString();
                options.KeyNamespace = TState.RedisKeyNamespace;
            });

            serviceCollection.Configure<RedisStateSessionOptions>(TestSessionOptions.Write,
                options => { options.ReadOnly = false; });

            serviceCollection.Configure<RedisStateSessionOptions>(TestSessionOptions.ReadOnly, options =>
            {
                options.ReadOnly = true;
                options.SecondaryPreferred = false;
            });

            serviceCollection.Configure<RedisStateSessionOptions>(TestSessionOptions.ReadOnlySecondaryPreferred,
                options =>
                {
                    options.ReadOnly = true;
                    options.SecondaryPreferred = true;
                });
        });
    }

    private static IEnumerable<StateRepositoryAdder> AllStateRepositoryAdders<TState>()
        where TState : class, IStateWithTestLogic<TState>
    {
        yield return RedisStateRepositoryAdder<TState>();
        yield return MongoDbStateRepositoryAdder<TState>();
    }

    private static EntityRepositoryAdder GetEntityRepositoryAdder<TEntity>()
        where TEntity : IEntity<TEntity>
    {
        return new EntityRepositoryAdder(typeof(TEntity).Name, typeof(TEntity),
            serviceCollection => { serviceCollection.AddEntityRepository<TEntity>(); });
    }

    private static IEnumerable<EntityRepositoryAdder> AllEntityRepositoryAdders()
    {
        yield return GetEntityRepositoryAdder<TestEntity>();
    }

    private static ProjectionRepositoryAdder GetProjectionRepositoryAdder<TProjection>()
        where TProjection : IProjection<TProjection>
    {
        return new ProjectionRepositoryAdder(typeof(TProjection).Name, typeof(TProjection), serviceCollection =>
        {
            serviceCollection.AddProjectionRepository<TProjection>();
        });
    }

    private static IEnumerable<ProjectionRepositoryAdder> AllProjectionRepositoryAdders()
    {
        yield return GetProjectionRepositoryAdder<OneToOneProjection>();
    }

    private static IEnumerable<StateRepositoryAdder> GetEntityStateRepositoryAdders<TEntity>()
        where TEntity : class, IEntity<TEntity>, IStateWithTestLogic<TEntity>
    {
        return
            from stateRepositoryAdder in AllStateRepositoryAdders<TEntity>()
            let entityRepositoryAdder = GetEntityRepositoryAdder<TEntity>()
            select stateRepositoryAdder with
            {
                AddDependencies = stateRepositoryAdder.AddDependencies + entityRepositoryAdder.AddDependencies +
                                  (serviceCollection =>
                                  {
                                      serviceCollection.AddEntityStateSourceSubscriber<TEntity>(
                                          TestSessionOptions.ReadOnly,
                                          TestSessionOptions.Write);
                                  }),
            };
    }

    private static IEnumerable<StateRepositoryAdder> GetProjectionStateRepositoryAdders<TProjection>()
        where TProjection : class, IProjection<TProjection>, IStateWithTestLogic<TProjection>
    {
        return
            from stateRepositoryAdder in AllStateRepositoryAdders<TProjection>()
            let projectionRepositoryAdder = GetProjectionRepositoryAdder<TProjection>()
            select stateRepositoryAdder with
            {
                AddDependencies = stateRepositoryAdder.AddDependencies + projectionRepositoryAdder.AddDependencies +
                                  (serviceCollection =>
                                  {
                                      serviceCollection.AddProjectionStateSourceSubscriber<TProjection>(
                                          TestSessionOptions.Write);
                                  }),
            };
    }

    private static IEnumerable<StateRepositoryAdder> AllEntityStateRepositoryAdders()
    {
        return Enumerable.Empty<StateRepositoryAdder>()
            .Concat(GetEntityStateRepositoryAdders<TestEntity>());
    }

    private static IEnumerable<StateRepositoryAdder> AllProjectionStateRepositoryAdders()
    {
        return Enumerable.Empty<StateRepositoryAdder>()
            .Concat(GetProjectionStateRepositoryAdders<OneToOneProjection>());
    }

    public static IEnumerable<object[]> With_Source_Entity()
    {
        return
            from sourceRepositoryAdder in AllSourceRepositoryAdders
            from entityRepositoryAdder in AllEntityRepositoryAdders()
            select new object[] { sourceRepositoryAdder, entityRepositoryAdder };
    }

    public static IEnumerable<object[]> With_Source()
    {
        return
            from sourceRepositoryAdder in AllSourceRepositoryAdders
            select new object[] { sourceRepositoryAdder };
    }

    public static IEnumerable<object[]> With_Entity()
    {
        return from entityRepositoryAdder in AllEntityRepositoryAdders()
            select new object[] { entityRepositoryAdder };
    }

    public static IEnumerable<object[]> With_EntityState()
    {
        return from entityStateRepositoryAdder in AllEntityStateRepositoryAdders()
            select new object[] { entityStateRepositoryAdder };
    }

    public static IEnumerable<object[]> With_ProjectionState()
    {
        return from projectionStateRepositoryAdder in AllProjectionStateRepositoryAdders()
            select new object[] { projectionStateRepositoryAdder };
    }

    public static IEnumerable<object[]> With_Source_EntityState()
    {
        return from sourceRepositoryAdder in AllSourceRepositoryAdders
            from entityStateRepositoryAdder in AllEntityStateRepositoryAdders()
            select new object[] { sourceRepositoryAdder, entityStateRepositoryAdder };
    }

    public static IEnumerable<object[]> With_Source_EntityState_ProjectionState()
    {
        return from sourceRepositoryAdder in AllSourceRepositoryAdders
            from entityStateRepositoryAdder in AllEntityStateRepositoryAdders()
            from projectionStateRepositoryAdder in AllProjectionStateRepositoryAdders()
            select new object[] { sourceRepositoryAdder, entityStateRepositoryAdder, projectionStateRepositoryAdder };
    }

    internal TestServiceScope CreateServiceScope(Action<IServiceCollection>? configureServices = null)
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

        if (_databaseContainerFixture != null)
        {
            serviceCollection.AddSingleton(_databaseContainerFixture);
        }

        configureServices?.Invoke(serviceCollection);

        serviceCollection.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));

        var singletonServiceProvider = serviceCollection.BuildServiceProvider();

        var serviceScopeFactory = singletonServiceProvider.GetRequiredService<IServiceScopeFactory>();

        return new TestServiceScope
        {
            SingletonServiceProvider = singletonServiceProvider,
            AsyncServiceScope = serviceScopeFactory.CreateAsyncScope(),
        };
    }

    protected static ILoggerFactory GetMockedLoggerFactory(List<Log> logs)
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);

        loggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns(new Logger(logs));

        loggerFactoryMock
            .Setup(factory => factory.AddProvider(It.IsAny<ILoggerProvider>()));

        return loggerFactoryMock.Object;
    }

    protected static ISourceSubscriber GetMockedSourceSubscriber(List<Source> committedSources)
    {
        var sourceSubscriberMock = new Mock<ISourceSubscriber>();

        sourceSubscriberMock
            .Setup(subscriber => subscriber.Notify(It.IsAny<Source>()))
            .Callback((Source source) =>
            {
                committedSources.Add(source);
            });

        return sourceSubscriberMock.Object;
    }

    protected static Task<IMultipleEntityRepository<TEntity>> GetWriteEntityRepository<TEntity>(
        IServiceScope serviceScope, bool getPersistedState)
    {
        return serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateMultiple
            (
                TestSessionOptions.Default,
                TestSessionOptions.Write,
                getPersistedState ? TestSessionOptions.Write : null
            );
    }

    protected static Task<IMultipleEntityRepository<TEntity>> GetReadOnlyEntityRepository<TEntity>(
        IServiceScope serviceScope, bool getPersistedState)
    {
        return serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateMultiple
            (
                TestSessionOptions.Default,
                TestSessionOptions.ReadOnly,
                getPersistedState ? TestSessionOptions.ReadOnly : null
            );
    }

    protected static Task<IMultipleStreamRepository> GetWriteStreamRepository(IServiceScope serviceScope)
    {
        return serviceScope.ServiceProvider
            .GetRequiredService<IStreamRepositoryFactory>()
            .CreateMultiple
            (
                TestSessionOptions.Default,
                TestSessionOptions.Write
            );
    }

    protected static Task<IMultipleStreamRepository> GetReadOnlyStreamRepository(IServiceScope serviceScope)
    {
        return serviceScope.ServiceProvider
            .GetRequiredService<IStreamRepositoryFactory>()
            .CreateMultiple
            (
                TestSessionOptions.Default,
                TestSessionOptions.ReadOnly
            );
    }

    protected static Task<IStateRepository<TState>> GetWriteStateRepository<TState>(
        IServiceScope serviceScope)
    {
        return serviceScope.ServiceProvider
            .GetRequiredService<IStateRepositoryFactory<TState>>()
            .Create(TestSessionOptions.Write);
    }

    protected static Task<IStateRepository<TState>> GetReadOnlyStateRepository<TState>(
        IServiceScope serviceScope, bool secondary = false)
    {
        return serviceScope.ServiceProvider
            .GetRequiredService<IStateRepositoryFactory<TState>>()
            .Create(secondary ? TestSessionOptions.ReadOnlySecondaryPreferred : TestSessionOptions.ReadOnly);
    }

    protected static Task<ISourceRepository> GetWriteSourceRepository(IServiceScope serviceScope)
    {
        return serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .Create
            (
                TestSessionOptions.Write
            );
    }

    protected static Task<ISourceRepository> GetReadOnlySourceRepository(IServiceScope serviceScope,
        bool secondary = false)
    {
        return serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .Create
            (
                secondary
                    ? TestSessionOptions.ReadOnlySecondaryPreferred
                    : TestSessionOptions.ReadOnly
            );
    }

    protected static Task<IProjectionRepository<TProjection>> GetReadOnlyProjectionRepository<TProjection>(
        IServiceScope serviceScope, bool getPersistedState)
    {
        return serviceScope.ServiceProvider
            .GetRequiredService<IProjectionRepositoryFactory<TProjection>>()
            .CreateRepository
            (
                getPersistedState ? TestSessionOptions.Write : null
            );
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
            .Setup(factory => factory.Create(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
            .Setup(repository =>
                repository.EnumerateDeltas(It.IsAny<IMessageDataQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerablePolyfill.FromResult(deltas));

        return GetMockedSourceRepositoryFactory(sourceRepositoryMock, new List<Source>());
    }

    protected static IStateRepositoryFactory<TEntity> GetMockedStateRepositoryFactory<TEntity>
    (
        TEntity? state = default
    )
    {
        var stateRepositoryMock = new Mock<IStateRepository<TEntity>>(MockBehavior.Strict);

        stateRepositoryMock
            .Setup(repository => repository.Get(It.IsAny<StatePointer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        stateRepositoryMock
            .Setup(repository => repository.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var stateRepositoryFactoryMock = new Mock<IStateRepositoryFactory<TEntity>>(MockBehavior.Strict);

        stateRepositoryFactoryMock
            .Setup(factory => factory.Create(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stateRepositoryMock.Object);

        stateRepositoryFactoryMock
            .Setup(factory => factory.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        return stateRepositoryFactoryMock.Object;
    }

    public sealed record Log
    {
        public required object[] ScopesStates { get; init; }
        public required LogLevel LogLevel { get; init; }
        public required EventId EventId { get; init; }
        public required object? State { get; init; }
        public required Exception? Exception { get; init; }
    }

    private sealed class Logger(ICollection<Log> logs) : ILogger
    {
        private readonly Stack<object> _scopeStates = new();

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            _scopeStates.Push(state);

            return new Scope(_scopeStates);
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            logs.Add(new Log
            {
                ScopesStates = _scopeStates.ToArray(),
                LogLevel = logLevel,
                EventId = eventId,
                State = state,
                Exception = exception,
            });
        }

        private sealed class Scope(Stack<object> scopeStates) : IDisposable
        {
            void IDisposable.Dispose()
            {
                scopeStates.Pop();
            }
        }
    }

    internal sealed record TestServiceScope : DisposableResourceBaseRecord, IServiceScope
    {
        public required ServiceProvider SingletonServiceProvider { get; init; }
        public required AsyncServiceScope AsyncServiceScope { get; init; }

        public IServiceProvider ServiceProvider => AsyncServiceScope.ServiceProvider;

        public override async ValueTask DisposeAsync()
        {
            await SingletonServiceProvider.DisposeAsync();
            await AsyncServiceScope.DisposeAsync();
        }
    }

    public sealed record SourceRepositoryAdder(string Name, Type QueryOptionsType,
        Func<TimeStamp, TimeStamp> FixTimeStamp,
        AddDependenciesDelegate AddDependencies)
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public sealed record StateRepositoryAdder(string Name, Type StateType, AddDependenciesDelegate AddDependencies)
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public sealed record EntityRepositoryAdder(string Name, Type EntityType, AddDependenciesDelegate AddDependencies)
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public sealed record ProjectionRepositoryAdder(string Name, Type ProjectionType,
        AddDependenciesDelegate AddDependencies)
    {
        public override string ToString()
        {
            return Name;
        }
    }
}
