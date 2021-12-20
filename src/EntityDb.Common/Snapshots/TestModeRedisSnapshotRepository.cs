using EntityDb.Abstractions.Snapshots;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots
{
    internal sealed class TestModeSnapshotRepository<TEntity> : ISnapshotRepository<TEntity>
    {
        private readonly ISnapshotRepository<TEntity> _snapshotRepository;
        private readonly SnapshotTestMode _snapshotTestMode;
        private readonly TestModeSnapshotDisposer _testModeSnapshotDisposer;

        public TestModeSnapshotRepository
        (
            ISnapshotRepository<TEntity> snapshotRepository,
            SnapshotTestMode snapshotTestMode,
            TestModeSnapshotDisposer testModeSnapshotDisposer
        )
        {
            _snapshotRepository = snapshotRepository;
            _snapshotTestMode = snapshotTestMode;
            _testModeSnapshotDisposer = testModeSnapshotDisposer;

            if (snapshotTestMode == SnapshotTestMode.AllRepositoriesDisposed)
            {
                _testModeSnapshotDisposer.Hold();
            }
        }

        public Task<bool> PutSnapshot(Guid entityId, TEntity entity)
        {
            _testModeSnapshotDisposer.AddEntityId(entityId);

            return _snapshotRepository.PutSnapshot(entityId, entity);
        }

        public Task<TEntity?> GetSnapshot(Guid entityId)
        {
            return _snapshotRepository.GetSnapshot(entityId);
        }

        public Task<bool> DeleteSnapshots(Guid[] entityIds)
        {
            _testModeSnapshotDisposer.RemoveEntityIds(entityIds);

            return _snapshotRepository.DeleteSnapshots(entityIds);
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            if (_snapshotTestMode == SnapshotTestMode.AllRepositoriesDisposed)
            {
                _testModeSnapshotDisposer.Release();

                if (_testModeSnapshotDisposer.NoHolds(out var deleteEntityIds))
                {
                    await DeleteSnapshots(deleteEntityIds);
                }
            }

            await _snapshotRepository.DisposeAsync();
        }
    }
}
