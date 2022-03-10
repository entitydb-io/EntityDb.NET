using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.InMemory.Sessions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.InMemory.Snapshots;

internal class InMemorySnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly IInMemorySession<TSnapshot> _inMemorySession;

    public InMemorySnapshotRepository(IInMemorySession<TSnapshot> inMemorySession)
    {
        _inMemorySession = inMemorySession;
    }

    public Task<bool> PutSnapshot(Id snapshotId, TSnapshot snapshot)
    {
        return _inMemorySession.Insert(snapshotId, snapshot);
    }

    public Task<TSnapshot?> GetSnapshot(Id snapshotId)
    {
        return _inMemorySession.Get(snapshotId);
    }

    public Task<bool> DeleteSnapshots(Id[] snapshotIds)
    {
        return _inMemorySession.Delete(snapshotIds);
    }
}
