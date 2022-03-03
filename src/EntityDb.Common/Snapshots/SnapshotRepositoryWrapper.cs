using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal abstract class SnapshotRepositoryWrapper<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly ISnapshotRepository<TSnapshot> _snapshotRepository;

    protected SnapshotRepositoryWrapper
    (
        ISnapshotRepository<TSnapshot> snapshotRepository
    )
    {
        _snapshotRepository = snapshotRepository;
    }

    public virtual Task<bool> PutSnapshot(Id snapshotId, TSnapshot snapshot)
    {
        return WrapCommand(_snapshotRepository.PutSnapshot(snapshotId, snapshot));
    }

    public virtual Task<TSnapshot?> GetSnapshot(Id snapshotId)
    {
        return WrapQuery(_snapshotRepository.GetSnapshot(snapshotId));
    }

    public virtual Task<bool> DeleteSnapshots(Id[] snapshotIds)
    {
        return WrapCommand(_snapshotRepository.DeleteSnapshots(snapshotIds));
    }

    public override async ValueTask DisposeAsync()
    {
        await _snapshotRepository.DisposeAsync();
    }

    protected abstract Task<TSnapshot?> WrapQuery(Task<TSnapshot?> task);

    protected abstract Task<bool> WrapCommand(Task<bool> task);
}
