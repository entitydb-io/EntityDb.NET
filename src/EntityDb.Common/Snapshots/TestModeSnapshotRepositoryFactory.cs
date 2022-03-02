using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal sealed class TestModeSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepositoryFactory<TSnapshot>
{
    private readonly ISnapshotRepositoryFactory<TSnapshot> _snapshotRepositoryFactory;
    private readonly TestModeSnapshotManager _testModeSnapshotManager = new();

    public TestModeSnapshotRepositoryFactory
    (
        ISnapshotRepositoryFactory<TSnapshot> snapshotRepositoryFactory
    )
    {
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
    }

    public async Task<ISnapshotRepository<TSnapshot>> CreateRepository(string snapshotSessionOptionsName)
    {
        var snapshotRepository = await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName);

        return new TestModeSnapshotRepository<TSnapshot>(snapshotRepository, _testModeSnapshotManager);
    }

    public override async ValueTask DisposeAsync()
    {
        var deleteSnapshotIds = _testModeSnapshotManager.GetDeleteSnapshotIds();

        var snapshotRepository = await CreateRepository("");

        await snapshotRepository.DeleteSnapshots(deleteSnapshotIds);

        await _snapshotRepositoryFactory.DisposeAsync();
    }
}
