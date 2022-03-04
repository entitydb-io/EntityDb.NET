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
    private readonly string _connectionString;
    private readonly string _keyNamespace;
    private readonly ILogger<ISnapshotRepositoryFactory<TSnapshot>> _logger;
    private readonly IEnvelopeService<JsonElement> _envelopeService;
    private readonly IOptionsFactory<SnapshotSessionOptions> _optionsFactory;

    public RedisSnapshotRepositoryFactory
    (
        IOptionsFactory<SnapshotSessionOptions> optionsFactory,
        ILogger<ISnapshotRepositoryFactory<TSnapshot>> logger,
        IEnvelopeService<JsonElement> envelopeService,
        ITypeResolver typeResolver,
        string connectionString,
        string keyNamespace
    )
    {
        _optionsFactory = optionsFactory;
        _logger = logger;
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

        return redisSnapshotRepository.UseTryCatch(_logger);
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
