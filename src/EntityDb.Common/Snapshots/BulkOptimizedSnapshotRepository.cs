using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal class BulkOptimizedSnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly Dictionary<Id, TSnapshot> _cache = new();
    private readonly Dictionary<Id, TSnapshot> _buffer = new();
    private readonly ISnapshotRepository<TSnapshot> _snapshotRepository;

    public BulkOptimizedSnapshotRepository(ISnapshotRepository<TSnapshot> snapshotRepository)
    {
        _snapshotRepository = snapshotRepository;
    }

    public Task<bool> DeleteSnapshots(Id[] snapshotIds, CancellationToken cancellationToken = default)
    {
        return _snapshotRepository.DeleteSnapshots(snapshotIds, cancellationToken);
    }

    public async Task<TSnapshot?> GetSnapshot(Id snapshotId, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(snapshotId, out var snapshot))
        {
            return snapshot;
        }

        snapshot = await _snapshotRepository.GetSnapshot(snapshotId, cancellationToken);

        if (snapshot is not null)
        {
            _cache[snapshotId] = snapshot;
        }

        return snapshot;
    }

    public Task<bool> PutSnapshot(Id snapshotId, TSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _cache[snapshotId] = snapshot;
        _buffer[snapshotId] = snapshot;

        return Task.FromResult(false);
    }

    public void CacheSnapshot(Id snapshotId, TSnapshot snapshot)
    {
        _cache[snapshotId] = snapshot;
    }

    public async Task<bool> PutSnapshots(CancellationToken cancellationToken = default)
    {
        foreach (var (snapshotId, snapshot) in _buffer)
        {
            await _snapshotRepository.PutSnapshot(snapshotId, snapshot, cancellationToken);
        }

        return true;
    }

    public override ValueTask DisposeAsync()
    {
        _cache.Clear();
        _buffer.Clear();

        return _snapshotRepository.DisposeAsync();
    }
}
