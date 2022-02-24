using EntityDb.Abstractions.Snapshots;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal sealed class TestModeSnapshotRepositoryFactory<TEntity> : ISnapshotRepositoryFactory<TEntity>
{
    private readonly ISnapshotRepositoryFactory<TEntity> _snapshotRepositoryFactory;
    private readonly TestModeSnapshotManager _testModeSnapshotManager = new();

    public TestModeSnapshotRepositoryFactory
    (
        ISnapshotRepositoryFactory<TEntity> snapshotRepositoryFactory
    )
    {
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
    }

    public async Task<ISnapshotRepository<TEntity>> CreateRepository(string snapshotSessionOptionsName)
    {
        var snapshotRepository = await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName);

        return new TestModeSnapshotRepository<TEntity>(snapshotRepository, _testModeSnapshotManager);
    }

    public async ValueTask DisposeAsync()
    {
        var deleteEntityIds = _testModeSnapshotManager.GetDeleteEntityIds();

        var snapshotRepository = await CreateRepository("");

        await snapshotRepository.DeleteSnapshots(deleteEntityIds);

        await _snapshotRepositoryFactory.DisposeAsync();
    }
}
