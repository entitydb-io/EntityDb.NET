using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.InMemory.Sessions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.InMemory.Snapshots;

internal class InMemorySnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly IInMemorySession<TSnapshot> _inMemorySession;

    public InMemorySnapshotRepository(IInMemorySession<TSnapshot> inMemorySession)
    {
        _inMemorySession = inMemorySession;
    }

    public Task<bool> PutSnapshot(Id snapshotId, TSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        return _inMemorySession.Insert(snapshotId, snapshot).WaitAsync(cancellationToken);
    }

    public Task<TSnapshot?> GetSnapshot(Id snapshotId, CancellationToken cancellationToken = default)
    {
        return _inMemorySession.Get(snapshotId).WaitAsync(cancellationToken);
    }

    public Task<bool> DeleteSnapshots(Id[] snapshotIds, CancellationToken cancellationToken = default)
    {
        return _inMemorySession.Delete(snapshotIds).WaitAsync(cancellationToken);
    }
}
