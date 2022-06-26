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

    private RedisKey GetSnapshotKey(Pointer snapshotPointer)
    {
        return $"{_keyNamespace}#{snapshotPointer.Id.Value}@{snapshotPointer.VersionNumber.Value}";
    }

    public async Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var snapshotKey = GetSnapshotKey(snapshotPointer);

        var snapshotValue = _envelopeService
            .DeconstructAndSerialize(snapshot); 

        return await _redisSession.Insert(snapshotKey, snapshotValue).WaitAsync(cancellationToken);
    }

    public async Task<TSnapshot?> GetSnapshot(Pointer snapshotPointer, CancellationToken cancellationToken = default)
    {
        var snapshotKey = GetSnapshotKey(snapshotPointer);
        var snapshotValue = await _redisSession.Find(snapshotKey).WaitAsync(cancellationToken);

        if (!snapshotValue.HasValue)
        {
            return default;
        }

        return _envelopeService
            .DeserializeAndReconstruct<JsonElement, TSnapshot>(snapshotValue);
    }

    public async Task<bool> DeleteSnapshots(Pointer[] snapshotPointers, CancellationToken cancellationToken = default)
    {
        var snapshotKeys = snapshotPointers.Select(GetSnapshotKey).ToArray();

        return await _redisSession.Delete(snapshotKeys).WaitAsync(cancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await _redisSession.DisposeAsync();
    }
}
