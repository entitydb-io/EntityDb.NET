using EntityDb.Abstractions.Snapshots;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots
{
    internal abstract class SnapshotRepositoryWrapper<TEntity> : ISnapshotRepository<TEntity>
    {
        private readonly ISnapshotRepository<TEntity> _snapshotRepository;

        protected SnapshotRepositoryWrapper
        (
            ISnapshotRepository<TEntity> snapshotRepository
        )
        {
            _snapshotRepository = snapshotRepository;
        }

        public virtual Task<bool> PutSnapshot(Guid entityId, TEntity entity)
        {
            return WrapCommand(_snapshotRepository.PutSnapshot(entityId, entity));
        }

        public virtual Task<TEntity?> GetSnapshot(Guid entityId)
        {
            return WrapQuery(_snapshotRepository.GetSnapshot(entityId));
        }

        public virtual Task<bool> DeleteSnapshots(Guid[] entityIds)
        {
            return WrapCommand(_snapshotRepository.DeleteSnapshots(entityIds));
        }

        public virtual ValueTask DisposeAsync()
        {
            return _snapshotRepository.DisposeAsync();
        }

        protected abstract Task<TEntity?> WrapQuery(Task<TEntity?> task);

        protected abstract Task<bool> WrapCommand(Task<bool> task);
    }
}
