using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Extensions;
using EntityDb.Redis.Sessions;
using StackExchange.Redis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly IEnvelopeService<JsonElement> _envelopeService;
    private readonly string _keyNamespace;
    private readonly IRedisSession _redisSession;

    public RedisSnapshotRepository
    (
        IEnvelopeService<JsonElement> envelopeService,
        string keyNamespace,
        IRedisSession redisSession
    )
    {
        _envelopeService = envelopeService;
        _keyNamespace = keyNamespace;
        _redisSession = redisSession;
    }

    private RedisKey GetSnapshotKey(Id snapshotId)
    {
        return $"{_keyNamespace}#{snapshotId.Value}";
    }

    public async Task<bool> PutSnapshot(Id snapshotId, TSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var snapshotKey = GetSnapshotKey(snapshotId);

        var snapshotValue = _envelopeService
            .DeconstructAndSerialize(snapshot);

        return await _redisSession.Insert(snapshotKey, snapshotValue).WaitAsync(cancellationToken);
    }

    public async Task<TSnapshot?> GetSnapshot(Id snapshotId, CancellationToken cancellationToken = default)
    {
        var snapshotKey = GetSnapshotKey(snapshotId);
        var snapshotValue = await _redisSession.Find(snapshotKey).WaitAsync(cancellationToken);

        if (!snapshotValue.HasValue)
        {
            return default;
        }

        return _envelopeService
            .DeserializeAndReconstruct<JsonElement, TSnapshot>(snapshotValue);
    }

    public async Task<bool> DeleteSnapshots(Id[] snapshotIds, CancellationToken cancellationToken = default)
    {
        var snapshotKeys = snapshotIds.Select(GetSnapshotKey).ToArray();

        return await _redisSession.Delete(snapshotKeys).WaitAsync(cancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await _redisSession.DisposeAsync();
    }
}
