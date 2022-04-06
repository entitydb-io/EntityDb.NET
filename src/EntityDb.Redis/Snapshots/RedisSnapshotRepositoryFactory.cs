using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Snapshots;
using EntityDb.Common.TypeResolvers;
using EntityDb.Redis.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepositoryFactory<TSnapshot>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsFactory<SnapshotSessionOptions> _optionsFactory;
    private readonly IEnvelopeService<JsonElement> _envelopeService;
    private readonly string _connectionString;
    private readonly string _keyNamespace;
    private readonly SemaphoreSlim _connectionSemaphore = new(1);
    
    private IConnectionMultiplexer? _connectionMultiplexer;

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

    private async Task<IConnectionMultiplexer> OpenConnectionMultiplexer(CancellationToken cancellationToken)
    {
        await _connectionSemaphore.WaitAsync(cancellationToken);
        
        if (_connectionMultiplexer != null)
        {
            _connectionSemaphore.Release();
            
            return _connectionMultiplexer;
        }
        
        var configurationOptions = ConfigurationOptions.Parse(_connectionString);
        
        _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions).WaitAsync(cancellationToken);

        _connectionSemaphore.Release();

        return _connectionMultiplexer;
    }

    private async Task<IRedisSession> CreateSession(SnapshotSessionOptions snapshotSessionOptions, CancellationToken cancellationToken)
    {
        var connectionMultiplexer = await OpenConnectionMultiplexer(cancellationToken);

        return RedisSession.Create(_serviceProvider, connectionMultiplexer.GetDatabase(), snapshotSessionOptions);
    }

    public async Task<ISnapshotRepository<TSnapshot>> CreateRepository(string snapshotSessionOptionsName, CancellationToken cancellationToken = default)
    {
        var snapshotSessionOptions = _optionsFactory.Create(snapshotSessionOptionsName);

        var redisSession = await CreateSession(snapshotSessionOptions, cancellationToken);

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

    public override ValueTask DisposeAsync()
    {
        _connectionMultiplexer?.Dispose();
        
        return ValueTask.CompletedTask;
    }
}
