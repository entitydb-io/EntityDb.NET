using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.TypeResolvers;
using EntityDb.Common.Disposables;
using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Redis.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepositoryFactory<TSnapshot>
{
    private readonly string _connectionString;
    private readonly string _keyNamespace;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptionsFactory<SnapshotSessionOptions> _optionsFactory;
    private readonly ITypeResolver _typeResolver;

    public RedisSnapshotRepositoryFactory(IOptionsFactory<SnapshotSessionOptions> optionsFactory,
        ILoggerFactory loggerFactory,
        ITypeResolver typeResolver, string connectionString, string keyNamespace)
    {
        _optionsFactory = optionsFactory;
        _loggerFactory = loggerFactory;
        _typeResolver = typeResolver;
        _connectionString = connectionString;
        _keyNamespace = keyNamespace;
    }

    private async Task<IRedisSession> CreateSession(SnapshotSessionOptions snapshotSessionOptions)
    {
        var configurationOptions = ConfigurationOptions.Parse(_connectionString);

        var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions);

        return new RedisSession(connectionMultiplexer, snapshotSessionOptions);
    }

    public async Task<ISnapshotRepository<TSnapshot>> CreateRepository(string snapshotSessionOptionsName)
    {
        var snapshotSessionOptions = _optionsFactory.Create(snapshotSessionOptionsName);

        var redisSession = await CreateSession(snapshotSessionOptions);

        var logger = _loggerFactory.CreateLogger<TSnapshot>();

        var redisSnapshotRepository = new RedisSnapshotRepository<TSnapshot>
        (
            _keyNamespace,
            _typeResolver,
            redisSession,
            logger
        );

        return redisSnapshotRepository.UseTryCatch(logger);
    }

    public static RedisSnapshotRepositoryFactory<TSnapshot> Create(IServiceProvider serviceProvider,
        string connectionString, string keyNamespace)
    {
        return ActivatorUtilities.CreateInstance<RedisSnapshotRepositoryFactory<TSnapshot>>
        (
            serviceProvider,
            connectionString,
            keyNamespace
        );
    }
}
