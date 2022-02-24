using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.TypeResolvers;
using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Redis.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepositoryFactory<TEntity> : ISnapshotRepositoryFactory<TEntity>
{
    private readonly string _connectionString;
    private readonly string _keyNamespace;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptionsFactory<SnapshotSessionOptions> _optionsFactory;
    private readonly ITypeResolver _typeResolver;
    private readonly ISnapshotStrategy<TEntity>? _snapshotStrategy;

    public RedisSnapshotRepositoryFactory(IOptionsFactory<SnapshotSessionOptions> optionsFactory,
        ILoggerFactory loggerFactory,
        ITypeResolver typeResolver, string connectionString, string keyNamespace,
        ISnapshotStrategy<TEntity>? snapshotStrategy = null)
    {
        _optionsFactory = optionsFactory;
        _loggerFactory = loggerFactory;
        _typeResolver = typeResolver;
        _connectionString = connectionString;
        _keyNamespace = keyNamespace;
        _snapshotStrategy = snapshotStrategy;
    }

    private async Task<IRedisSession> CreateSession(SnapshotSessionOptions snapshotSessionOptions)
    {
        var configurationOptions = ConfigurationOptions.Parse(_connectionString);

        var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions);

        return new RedisSession(connectionMultiplexer, snapshotSessionOptions);
    }

    public async Task<ISnapshotRepository<TEntity>> CreateRepository(string snapshotSessionOptionsName)
    {
        var snapshotSessionOptions = _optionsFactory.Create(snapshotSessionOptionsName);

        var redisSession = await CreateSession(snapshotSessionOptions);

        var logger = _loggerFactory.CreateLogger<TEntity>();

        var redisSnapshotRepository = new RedisSnapshotRepository<TEntity>
        (
            _keyNamespace,
            _typeResolver,
            _snapshotStrategy,
            redisSession,
            logger
        );

        return redisSnapshotRepository.UseTryCatch(logger);
    }

    public static RedisSnapshotRepositoryFactory<TEntity> Create(IServiceProvider serviceProvider,
        string connectionString, string keyNamespace)
    {
        return ActivatorUtilities.CreateInstance<RedisSnapshotRepositoryFactory<TEntity>>
        (
            serviceProvider,
            connectionString,
            keyNamespace
        );
    }
}
