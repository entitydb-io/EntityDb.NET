using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Entities
{
    /// <summary>
    /// Encapsulates the transaction repository and the snapshot repository of an entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IEntityRepository<TEntity> : IDisposable, IAsyncDisposable
    {
        /// <inheritdoc cref="ITransactionRepository{TEntity}"/>
        ITransactionRepository<TEntity> TransactionRepository { get; }

        /// <inheritdoc cref="ISnapshotRepository{TEntity}" />
        ISnapshotRepository<TEntity>? SnapshotRepository { get; }

        /// <summary>
        /// Returns a <typeparamref name="TEntity"/> snapshot or <c>default(<typeparamref name="TEntity"/>)</c>.
        /// </summary>
        /// <param name="entityId">The id of the entity.</param>
        /// <returns>A <typeparamref name="TEntity"/> snapshot or <c>default(<typeparamref name="TEntity"/>)</c>.</returns>
        Task<TEntity> Get(Guid entityId);

        /// <summary>
        /// Inserts a single transaction with an atomic commit.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns><c>true</c> if the insert suceeded, or <c>false</c> if the insert failed.</returns>
        Task<bool> Put(ITransaction<TEntity> transaction);
    }
}
