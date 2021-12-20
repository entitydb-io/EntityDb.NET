using EntityDb.Abstractions.Snapshots;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots
{
    internal sealed class TestModeSnapshotRepositoryFactory<TEntity> : ISnapshotRepositoryFactory<TEntity>
    {
        private readonly ISnapshotRepositoryFactory<TEntity> _snapshotRepositoryFactory;
        private readonly SnapshotTestMode _snapshotTestMode;
        private readonly TestModeSnapshotDisposer _testModeSnapshotDisposer = new();

        public TestModeSnapshotRepositoryFactory
        (
            ISnapshotRepositoryFactory<TEntity> snapshotRepositoryFactory,
            SnapshotTestMode snapshotTestMode
        )
        {
            _snapshotRepositoryFactory = snapshotRepositoryFactory;
            _snapshotTestMode = snapshotTestMode;
        }

        public async Task<ISnapshotRepository<TEntity>> CreateRepository(string snapshotSessionOptionsName)
        {
            var snapshotRepository = await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName);

            return new TestModeSnapshotRepository<TEntity>(snapshotRepository, _snapshotTestMode, _testModeSnapshotDisposer);
        }

        [ExcludeFromCodeCoverage(Justification = "Proxy for DisposeAsync")]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            if (_snapshotTestMode == SnapshotTestMode.AllRepositoriesDisposed && _testModeSnapshotDisposer.NoHolds(out var deleteEntityIds))
            {
                var snapshotRepository = await _snapshotRepositoryFactory.CreateRepository("TODO");

                await snapshotRepository.DeleteSnapshots(deleteEntityIds);
            }

            await _snapshotRepositoryFactory.DisposeAsync();
        }
    }
}
