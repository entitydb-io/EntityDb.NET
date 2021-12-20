using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Snapshots
{
    /// <summary>
    ///     Represents a collection of <typeparamref name="TEntity" /> snapshots.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity stored in the <see cref="ISnapshotRepository{TEntity}" />.</typeparam>
    public interface ISnapshotRepository<TEntity> : IDisposable, IAsyncDisposable
    {
        /// <summary>
        ///     Returns a <typeparamref name="TEntity" /> snapshot or <c>default(<typeparamref name="TEntity" />)</c>.
        /// </summary>
        /// <param name="entityId">The id of the entity.</param>
        /// <returns>A <typeparamref name="TEntity" /> snapshot or <c>default(<typeparamref name="TEntity" />)</c>.</returns>
        Task<TEntity?> GetSnapshot(Guid entityId);

        /// <summary>
        ///     Inserts a <typeparamref name="TEntity" /> snapshot.
        /// </summary>
        /// <param name="entityId">The id of the entity.</param>
        /// <param name="entity">The entity.</param>
        /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
        Task<bool> PutSnapshot(Guid entityId, TEntity entity);

        /// <summary>
        ///     Deletes multiple <typeparamref name="TEntity" /> snapshots.
        /// </summary>
        /// <param name="entityIds">The id of the entitie snapshots to delete.</param>
        /// <returns><c>true</c> if the deletes all succeeded, or <c>false</c> if any deletes failed.</returns>
        Task<bool> DeleteSnapshots(Guid[] entityIds);
    }
}
