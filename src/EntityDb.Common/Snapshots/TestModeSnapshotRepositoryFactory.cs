using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal sealed class TestModeSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepositoryFactory<TSnapshot>
{
    private readonly ISnapshotRepositoryFactory<TSnapshot> _snapshotRepositoryFactory;
    private readonly TestModeSnapshotManager<TSnapshot> _testModeSnapshotManager = new();

    public TestModeSnapshotRepositoryFactory
    (
        ISnapshotRepositoryFactory<TSnapshot> snapshotRepositoryFactory
    )
    {
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
    }

    public async Task<ISnapshotRepository<TSnapshot>> CreateRepository(string snapshotSessionOptionsName, CancellationToken cancellationToken = default)
    {
        var snapshotRepository = await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName, cancellationToken);

        return new TestModeSnapshotRepository<TSnapshot>(snapshotRepository, _testModeSnapshotManager);
    }

    public override async ValueTask DisposeAsync()
    {
        await _testModeSnapshotManager.DisposeAsync();
        await _snapshotRepositoryFactory.DisposeAsync();
    }
}
