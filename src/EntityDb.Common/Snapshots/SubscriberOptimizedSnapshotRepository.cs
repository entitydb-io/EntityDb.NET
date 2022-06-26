using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal class SubscriberOptimizedSnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
    where TSnapshot : ISnapshot<TSnapshot>
{
    private readonly Dictionary<Pointer, TSnapshot> _getCache = new();
    private readonly Dictionary<Pointer, TSnapshot> _putQueue = new();
    private readonly ISnapshotRepository<TSnapshot> _snapshotRepository;

    public SubscriberOptimizedSnapshotRepository(ISnapshotRepository<TSnapshot> snapshotRepository)
    {
        _snapshotRepository = snapshotRepository;
    }

    public Task<bool> DeleteSnapshots(Pointer[] snapshotPointers, CancellationToken cancellationToken = default)
    {
        return _snapshotRepository.DeleteSnapshots(snapshotPointers, cancellationToken);
    }

    public async Task<TSnapshot?> GetSnapshot(Pointer snapshotPointer, CancellationToken cancellationToken = default)
    {
        if (_getCache.TryGetValue(snapshotPointer, out var snapshot))
        {
            return snapshot;
        }

        snapshot = await _snapshotRepository.GetSnapshot(snapshotPointer, cancellationToken);

        if (snapshot is not null)
        {
            _getCache[snapshotPointer] = snapshot;
        }

        return snapshot;
    }

    public Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _getCache[snapshotPointer] = snapshot;
        _putQueue[snapshotPointer] = snapshot;

        return Task.FromResult(false);
    }

    public void CacheSnapshot(Pointer snapshotPointer, TSnapshot snapshot)
    {
        _getCache[snapshotPointer] = snapshot;
    }

    public async Task<bool> PutSnapshots(CancellationToken cancellationToken = default)
    {
        foreach (var (snapshotPointer, snapshot) in _putQueue)
        {
            await _snapshotRepository.PutSnapshot(snapshotPointer, snapshot, cancellationToken);
        }

        return true;
    }

    public override ValueTask DisposeAsync()
    {
        _getCache.Clear();
        _putQueue.Clear();

        return _snapshotRepository.DisposeAsync();
    }
}
