using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Snapshots;
using EntityDb.Redis.ConnectionMultiplexers;
using EntityDb.Redis.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepositoryFactory<TSnapshot>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConnectionMultiplexerFactory _connectionMultiplexerFactory;
    private readonly IOptionsFactory<SnapshotSessionOptions> _optionsFactory;
    private readonly IEnvelopeService<JsonElement> _envelopeService;
    private readonly string _connectionString;
    private readonly string _keyNamespace;

    public RedisSnapshotRepositoryFactory
    (
        IServiceProvider serviceProvider,
        ConnectionMultiplexerFactory connectionMultiplexerFactory,
        IOptionsFactory<SnapshotSessionOptions> optionsFactory,
        IEnvelopeService<JsonElement> envelopeService,
        string connectionString,
        string keyNamespace
    )
    {
        _serviceProvider = serviceProvider;
        _connectionMultiplexerFactory = connectionMultiplexerFactory;
        _optionsFactory = optionsFactory;
        _envelopeService = envelopeService;
        _connectionString = connectionString;
        _keyNamespace = keyNamespace;
    }


    private async Task<IRedisSession> CreateSession(SnapshotSessionOptions snapshotSessionOptions, CancellationToken cancellationToken)
    {
        var connectionMultiplexer = await _connectionMultiplexerFactory.CreateConnectionMultiplexer(_connectionString, cancellationToken);

        return RedisSession.Create(_serviceProvider, connectionMultiplexer.GetDatabase(), _keyNamespace, snapshotSessionOptions);
    }

    public async Task<ISnapshotRepository<TSnapshot>> CreateRepository(string snapshotSessionOptionsName, CancellationToken cancellationToken = default)
    {
        var snapshotSessionOptions = _optionsFactory.Create(snapshotSessionOptionsName);

        var redisSession = await CreateSession(snapshotSessionOptions, cancellationToken);

        var redisSnapshotRepository = new RedisSnapshotRepository<TSnapshot>
        (
            _envelopeService,
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
