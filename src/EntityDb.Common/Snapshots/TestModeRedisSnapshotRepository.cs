using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.Snapshots;

internal sealed class TestModeSnapshotRepository<TSnapshot> : DisposableResourceBaseClass,
    ISnapshotRepository<TSnapshot>
{
    private readonly ISnapshotRepository<TSnapshot> _snapshotRepository;
    private readonly TestModeSnapshotManager<TSnapshot> _testModeSnapshotManager;

    public TestModeSnapshotRepository
    (
        ISnapshotRepository<TSnapshot> snapshotRepository,
        TestModeSnapshotManager<TSnapshot> testModeSnapshotManager
    )
    {
        _snapshotRepository = snapshotRepository;
        _testModeSnapshotManager = testModeSnapshotManager;
    }

    public Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        _testModeSnapshotManager.AddSnapshotPointer(this, snapshotPointer);

        return _snapshotRepository.PutSnapshot(snapshotPointer, snapshot, cancellationToken);
    }

    public Task<TSnapshot?> GetSnapshotOrDefault(Pointer snapshotPointer, CancellationToken cancellationToken = default)
    {
        return _snapshotRepository.GetSnapshotOrDefault(snapshotPointer, cancellationToken);
    }

    public Task<bool> DeleteSnapshots(Pointer[] snapshotPointers, CancellationToken cancellationToken = default)
    {
        _testModeSnapshotManager.RemoveSnapshotPointers(this, snapshotPointers);

        return _snapshotRepository.DeleteSnapshots(snapshotPointers, cancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await _snapshotRepository.DisposeAsync();
    }
}
