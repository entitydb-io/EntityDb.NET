using EntityDb.Abstractions.Transactions;
using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Entities
{
    /// <summary>
    ///     Encapsulates the transaction repository and the snapshot repository of an entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IEntityRepository<TEntity> : IDisposable, IAsyncDisposable
    {
        /// <summary>
        ///     Returns the most recent snapshot of a <typeparamref name="TEntity" /> or <c>default(<typeparamref name="TEntity" />)</c>.
        /// </summary>
        /// <param name="entityId">The id of the entity.</param>
        /// <returns>The most recent snapshot of a <typeparamref name="TEntity" /> or constructs a new <typeparamref name="TEntity" />.</returns>
        Task<TEntity?> GetSnapshotOrDefault(Guid entityId);
        
        /// <summary>
        ///     Returns the current state of a <typeparamref name="TEntity" /> or constructs a new <typeparamref name="TEntity" />.
        /// </summary>
        /// <param name="entityId">The id of the entity.</param>
        /// <returns>The current state of a <typeparamref name="TEntity" /> or constructs a new <typeparamref name="TEntity" />.</returns>
        Task<TEntity> GetCurrent(Guid entityId);

        /// <summary>
        ///     Inserts a single transaction with an atomic commit.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
        Task<bool> PutTransaction(ITransaction<TEntity> transaction);
    }
}
