using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots
{
    internal sealed class TryCatchSnapshotRepository<TEntity> : ISnapshotRepository<TEntity>
    {
        private readonly ISnapshotRepository<TEntity> _snapshotRepository;
        private readonly ILogger _logger;

        public TryCatchSnapshotRepository
        (
            ISnapshotRepository<TEntity> snapshotRepository,
            ILogger logger
        )
        {
            _snapshotRepository = snapshotRepository;
            _logger = logger;
        }

        public async Task<T?> TryCatchQuery<T>(Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "The operation cannot be completed.");

                return default;
            }
        }

        public async Task<T> TryCatchCommand<T>(Task<T> task)
            where T : struct
        {
            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "The operation cannot be completed.");

                return default;
            }
        }

        public Task<bool> PutSnapshot(Guid entityId, TEntity entity)
        {
            return TryCatchQuery(_snapshotRepository.PutSnapshot(entityId, entity));
        }

        public Task<TEntity?> GetSnapshot(Guid entityId)
        {
            return TryCatchQuery(_snapshotRepository.GetSnapshot(entityId));
        }

        public Task<bool> DeleteSnapshots(Guid[] entityIds)
        {
            return TryCatchQuery(_snapshotRepository.DeleteSnapshots(entityIds));
        }

        [ExcludeFromCodeCoverage(Justification = "Proxy for DisposeAsync")]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public ValueTask DisposeAsync()
        {
            return _snapshotRepository.DisposeAsync();
        }
    }
}
