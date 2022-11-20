using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.EntityFramework.Sessions;

namespace EntityDb.EntityFramework.Snapshots;

internal class EntityFrameworkSnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly IEntityFrameworkSession<TSnapshot> _entityFrameworkSession;

    public EntityFrameworkSnapshotRepository(IEntityFrameworkSession<TSnapshot> entityFrameworkSession)
    {
        _entityFrameworkSession = entityFrameworkSession;
    }

    public async Task<bool> DeleteSnapshots(Pointer[] snapshotPointers, CancellationToken cancellationToken = default)
    {
        await _entityFrameworkSession.Delete(snapshotPointers, cancellationToken);

        return true;
    }

    public Task<TSnapshot?> GetSnapshotOrDefault(Pointer snapshotPointer, CancellationToken cancellationToken = default)
    {
        return _entityFrameworkSession.Get(snapshotPointer, cancellationToken);
    }

    public async Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _entityFrameworkSession.Insert(snapshotPointer, snapshot, cancellationToken);

        return true;
    }
}
