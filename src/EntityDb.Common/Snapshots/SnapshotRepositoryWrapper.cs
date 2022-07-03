using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal abstract class SnapshotRepositoryWrapper<TSnapshot> : DisposableResourceBaseClass,
    ISnapshotRepository<TSnapshot>
{
    private readonly ISnapshotRepository<TSnapshot> _snapshotRepository;

    protected SnapshotRepositoryWrapper
    (
        ISnapshotRepository<TSnapshot> snapshotRepository
    )
    {
        _snapshotRepository = snapshotRepository;
    }

    public virtual Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        return WrapCommand(() => _snapshotRepository.PutSnapshot(snapshotPointer, snapshot, cancellationToken));
    }

    public virtual Task<TSnapshot?> GetSnapshotOrDefault(Pointer snapshotPointer,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _snapshotRepository.GetSnapshotOrDefault(snapshotPointer, cancellationToken));
    }

    public virtual Task<bool> DeleteSnapshots(Pointer[] snapshotPointers, CancellationToken cancellationToken = default)
    {
        return WrapCommand(() => _snapshotRepository.DeleteSnapshots(snapshotPointers, cancellationToken));
    }

    public override async ValueTask DisposeAsync()
    {
        await _snapshotRepository.DisposeAsync();
    }

    protected abstract Task<TSnapshot?> WrapQuery(Func<Task<TSnapshot?>> task);

    protected abstract Task<bool> WrapCommand(Func<Task<bool>> task);
}
