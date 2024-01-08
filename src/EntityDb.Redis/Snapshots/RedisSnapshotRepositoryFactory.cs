using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Snapshots;
using EntityDb.Redis.ConnectionMultiplexers;
using EntityDb.Redis.Snapshots.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass,
    ISnapshotRepositoryFactory<TSnapshot>
{
    private readonly ConnectionMultiplexerFactory _connectionMultiplexerFactory;
    private readonly IEnvelopeService<byte[]> _envelopeService;
    private readonly IOptionsFactory<RedisSnapshotSessionOptions> _optionsFactory;
    private readonly IServiceProvider _serviceProvider;

    public RedisSnapshotRepositoryFactory
    (
        IServiceProvider serviceProvider,
        ConnectionMultiplexerFactory connectionMultiplexerFactory,
        IOptionsFactory<RedisSnapshotSessionOptions> optionsFactory,
        IEnvelopeService<byte[]> envelopeService
    )
    {
        _serviceProvider = serviceProvider;
        _connectionMultiplexerFactory = connectionMultiplexerFactory;
        _optionsFactory = optionsFactory;
        _envelopeService = envelopeService;
    }

    public async Task<ISnapshotRepository<TSnapshot>> CreateRepository(string snapshotSessionOptionsName,
        CancellationToken cancellationToken = default)
    {
        var options = _optionsFactory.Create(snapshotSessionOptionsName);

        var redisSession = await CreateSession(options, cancellationToken);

        var redisSnapshotRepository = new RedisSnapshotRepository<TSnapshot>
        (
            _envelopeService,
            redisSession
        );

        return TryCatchSnapshotRepository<TSnapshot>.Create(_serviceProvider, redisSnapshotRepository);
    }


    private async Task<IRedisSession> CreateSession(RedisSnapshotSessionOptions options,
        CancellationToken cancellationToken)
    {
        var connectionMultiplexer =
            await _connectionMultiplexerFactory.CreateConnectionMultiplexer(options.ConnectionString,
                cancellationToken);

        return RedisSession.Create(_serviceProvider, connectionMultiplexer.GetDatabase(), options);
    }
}
