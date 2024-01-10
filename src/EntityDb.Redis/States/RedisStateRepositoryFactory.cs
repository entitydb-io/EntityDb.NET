using EntityDb.Abstractions.States;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.States;
using EntityDb.Redis.ConnectionMultiplexers;
using EntityDb.Redis.States.Sessions;
using Microsoft.Extensions.Options;

namespace EntityDb.Redis.States;

internal class RedisStateRepositoryFactory<TState> : DisposableResourceBaseClass,
    IStateRepositoryFactory<TState>
{
    private readonly ConnectionMultiplexerFactory _connectionMultiplexerFactory;
    private readonly IEnvelopeService<byte[]> _envelopeService;
    private readonly IOptionsFactory<RedisStateSessionOptions> _optionsFactory;
    private readonly IServiceProvider _serviceProvider;

    public RedisStateRepositoryFactory
    (
        IServiceProvider serviceProvider,
        ConnectionMultiplexerFactory connectionMultiplexerFactory,
        IOptionsFactory<RedisStateSessionOptions> optionsFactory,
        IEnvelopeService<byte[]> envelopeService
    )
    {
        _serviceProvider = serviceProvider;
        _connectionMultiplexerFactory = connectionMultiplexerFactory;
        _optionsFactory = optionsFactory;
        _envelopeService = envelopeService;
    }

    public async Task<IStateRepository<TState>> Create(string stateSessionOptionsName,
        CancellationToken cancellationToken = default)
    {
        var options = _optionsFactory.Create(stateSessionOptionsName);

        var redisSession = await CreateSession(options, cancellationToken);

        var redisStateRepository = new RedisStateRepository<TState>
        (
            _envelopeService,
            redisSession
        );

        return TryCatchStateRepository<TState>.Create(_serviceProvider, redisStateRepository);
    }


    private async Task<IRedisSession> CreateSession(RedisStateSessionOptions options,
        CancellationToken cancellationToken)
    {
        var connectionMultiplexer =
            await _connectionMultiplexerFactory.CreateConnectionMultiplexer(options.ConnectionString,
                cancellationToken);

        return RedisSession.Create(_serviceProvider, connectionMultiplexer.GetDatabase(), options);
    }
}
