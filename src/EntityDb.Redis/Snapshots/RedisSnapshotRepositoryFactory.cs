using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Common.TypeResolvers;
using EntityDb.Redis.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepositoryFactory<TSnapshot>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsFactory<SnapshotSessionOptions> _optionsFactory;
    private readonly IEnvelopeService<JsonElement> _envelopeService;
    private readonly string _connectionString;
    private readonly string _keyNamespace;

    public RedisSnapshotRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IOptionsFactory<SnapshotSessionOptions> optionsFactory,
        IEnvelopeService<JsonElement> envelopeService,
        ITypeResolver typeResolver,
        string connectionString,
        string keyNamespace
    )
    {
        _serviceProvider = serviceProvider;
        _optionsFactory = optionsFactory;
        _envelopeService = envelopeService;
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

        var redisSnapshotRepository = new RedisSnapshotRepository<TSnapshot>
        (
            _envelopeService,
            _keyNamespace,
            redisSession
        );
        
        return TryCatchSnapshotRepository<TSnapshot>.Create(_serviceProvider, redisSnapshotRepository);
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
