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
        try
        {
            await _entityFrameworkSession.StartTransaction(cancellationToken);

            await _entityFrameworkSession.Delete(snapshotPointers, cancellationToken);

            await _entityFrameworkSession.CommitTransaction(cancellationToken);

            return true;
        }
        catch
        {
            await _entityFrameworkSession.AbortTransaction(cancellationToken);

            throw;
        }
    }

    public Task<TSnapshot?> GetSnapshotOrDefault(Pointer snapshotPointer, CancellationToken cancellationToken = default)
    {
        return _entityFrameworkSession.Get(snapshotPointer, cancellationToken);
    }

    public async Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        try
        {
            await _entityFrameworkSession.StartTransaction(cancellationToken);

            await _entityFrameworkSession.Upsert(snapshotPointer, snapshot, cancellationToken);

            await _entityFrameworkSession.CommitTransaction(cancellationToken);

            return true;
        }
        catch
        {
            await _entityFrameworkSession.AbortTransaction(cancellationToken);

            throw;
        }
    }
}
