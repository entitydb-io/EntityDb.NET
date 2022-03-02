using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal sealed class TestModeSnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly ISnapshotRepository<TSnapshot> _snapshotRepository;
    private readonly TestModeSnapshotManager _testModeSnapshotManager;

    public TestModeSnapshotRepository
    (
        ISnapshotRepository<TSnapshot> snapshotRepository,
        TestModeSnapshotManager testModeSnapshotManager
    )
    {
        _snapshotRepository = snapshotRepository;
        _testModeSnapshotManager = testModeSnapshotManager;
    }

    public Task<bool> PutSnapshot(Guid snapshotId, TSnapshot snapshot)
    {
        _testModeSnapshotManager.AddSnapshotId(snapshotId);

        return _snapshotRepository.PutSnapshot(snapshotId, snapshot);
    }

    public Task<TSnapshot?> GetSnapshot(Guid snapshotId)
    {
        return _snapshotRepository.GetSnapshot(snapshotId);
    }

    public Task<bool> DeleteSnapshots(Guid[] snapshotIds)
    {
        _testModeSnapshotManager.RemoveSnapshotIds(snapshotIds);

        return _snapshotRepository.DeleteSnapshots(snapshotIds);
    }

    public override async ValueTask DisposeAsync()
    {
        await _snapshotRepository.DisposeAsync();
    }
}
