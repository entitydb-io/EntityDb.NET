using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System;
using System.Threading;
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

    public virtual Task<bool> PutSnapshot(Id snapshotId, TSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        return WrapCommand(() => _snapshotRepository.PutSnapshot(snapshotId, snapshot, cancellationToken));
    }

    public virtual Task<TSnapshot?> GetSnapshot(Id snapshotId, CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _snapshotRepository.GetSnapshot(snapshotId, cancellationToken));
    }

    public virtual Task<bool> DeleteSnapshots(Id[] snapshotIds, CancellationToken cancellationToken = default)
    {
        return WrapCommand(() => _snapshotRepository.DeleteSnapshots(snapshotIds, cancellationToken));
    }

    public override async ValueTask DisposeAsync()
    {
        await _snapshotRepository.DisposeAsync();
    }

    protected abstract Task<TSnapshot?> WrapQuery(Func<Task<TSnapshot?>> task);

    protected abstract Task<bool> WrapCommand(Func<Task<bool>> task);
}
