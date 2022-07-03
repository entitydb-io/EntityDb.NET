using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Extensions;
using EntityDb.Redis.Sessions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly IEnvelopeService<JsonElement> _envelopeService;
    private readonly IRedisSession _redisSession;

    public RedisSnapshotRepository
    (
        IEnvelopeService<JsonElement> envelopeService,
        IRedisSession redisSession
    )
    {
        _envelopeService = envelopeService;
        _redisSession = redisSession;
    }

    public async Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        var snapshotValue = _envelopeService
            .DeconstructAndSerialize(snapshot);

        return await _redisSession.Insert(snapshotPointer, snapshotValue).WaitAsync(cancellationToken);
    }

    public async Task<TSnapshot?> GetSnapshotOrDefault(Pointer snapshotPointer,
        CancellationToken cancellationToken = default)
    {
        var snapshotValue = await _redisSession.Find(snapshotPointer).WaitAsync(cancellationToken);

        if (!snapshotValue.HasValue)
        {
            return default;
        }

        return _envelopeService
            .DeserializeAndReconstruct<JsonElement, TSnapshot>(snapshotValue);
    }

    public Task<bool> DeleteSnapshots(Pointer[] snapshotPointers, CancellationToken cancellationToken = default)
    {
        return _redisSession.Delete(snapshotPointers).WaitAsync(cancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await _redisSession.DisposeAsync();
    }
}
