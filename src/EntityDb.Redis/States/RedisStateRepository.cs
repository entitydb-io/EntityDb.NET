using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Redis.States.Sessions;

namespace EntityDb.Redis.States;

internal sealed class RedisStateRepository<TState> : DisposableResourceBaseClass, IStateRepository<TState>
{
    private readonly IEnvelopeService<byte[]> _envelopeService;
    private readonly IRedisSession _redisSession;

    public RedisStateRepository
    (
        IEnvelopeService<byte[]> envelopeService,
        IRedisSession redisSession
    )
    {
        _envelopeService = envelopeService;
        _redisSession = redisSession;
    }

    public async Task<bool> Put(Pointer statePointer, TState state,
        CancellationToken cancellationToken = default)
    {
        var stateValue = _envelopeService
            .Serialize(state);

        return await _redisSession.Upsert(statePointer, stateValue).WaitAsync(cancellationToken);
    }

    public async Task<TState?> Get(Pointer statePointer,
        CancellationToken cancellationToken = default)
    {
        var stateValue = await _redisSession.Fetch(statePointer).WaitAsync(cancellationToken);

        if (!stateValue.HasValue)
        {
            return default;
        }

        return _envelopeService
            .Deserialize<TState>(stateValue!);
    }

    public Task<bool> Delete(Pointer[] statePointers, CancellationToken cancellationToken = default)
    {
        return _redisSession.Delete(statePointers).WaitAsync(cancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await _redisSession.DisposeAsync();
    }
}
