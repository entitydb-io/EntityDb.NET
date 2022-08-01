using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.InMemory.Sessions;

namespace EntityDb.InMemory.Snapshots;

internal class InMemorySnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly IInMemorySession<TSnapshot> _inMemorySession;

    public InMemorySnapshotRepository(IInMemorySession<TSnapshot> inMemorySession)
    {
        _inMemorySession = inMemorySession;
    }

    public Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        return _inMemorySession.Insert(snapshotPointer, snapshot).WaitAsync(cancellationToken);
    }

    public Task<TSnapshot?> GetSnapshotOrDefault(Pointer snapshotPointer, CancellationToken cancellationToken = default)
    {
        return _inMemorySession.Get(snapshotPointer).WaitAsync(cancellationToken);
    }

    public Task<bool> DeleteSnapshots(Pointer[] snapshotPointers, CancellationToken cancellationToken = default)
    {
        return _inMemorySession.Delete(snapshotPointers).WaitAsync(cancellationToken);
    }
}
