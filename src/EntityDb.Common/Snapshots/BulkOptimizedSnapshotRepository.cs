using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal class BulkOptimizedSnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly Dictionary<Id, TSnapshot> _getCache = new();
    private readonly Dictionary<Id, TSnapshot> _putCache = new();
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
        if (_getCache.TryGetValue(snapshotId, out var snapshot))
        {
            return snapshot;
        }

        snapshot = await _snapshotRepository.GetSnapshot(snapshotId, cancellationToken);

        if (snapshot != null)
        {
            _getCache[snapshotId] = snapshot;
        }

        return snapshot;
    }

    public Task<bool> PutSnapshot(Id snapshotId, TSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _getCache[snapshotId] = snapshot;
        _putCache[snapshotId] = snapshot;

        return Task.FromResult(false);
    }

    public void CacheSnapshot(Id snapshotId, TSnapshot snapshot)
    {
        _getCache[snapshotId] = snapshot;
    }

    public async Task<bool> PutSnapshots(CancellationToken cancellationToken = default)
    {
        foreach (var (snapshotId, snapshot) in _putCache)
        {
            await _snapshotRepository.PutSnapshot(snapshotId, snapshot, cancellationToken);
        }

        return true;
    }

    public override ValueTask DisposeAsync()
    {
        _getCache.Clear();
        _putCache.Clear();

        return _snapshotRepository.DisposeAsync();
    }
}
