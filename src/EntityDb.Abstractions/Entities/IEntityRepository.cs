using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Encapsulates the transaction repository and the snapshot repository of an entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IEntityRepository<TEntity> : IDisposableResource
{
    /// <summary>
    ///     The backing transaction repository.
    /// </summary>
    ITransactionRepository TransactionRepository { get; }

    /// <summary>
    ///     The backing snapshot repository (if snapshot is available).
    /// </summary>
    ISnapshotRepository<TEntity>? SnapshotRepository { get; }

    /// <summary>
    ///     Returns the snapshot of a <typeparamref name="TEntity" /> for a given <see cref="Pointer" />.
    /// </summary>
    /// <param name="entityPointer">A pointer to the entity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The snapshot of a <typeparamref name="TEntity" /> for <paramref name="entityPointer" />.</returns>
    Task<TEntity> GetSnapshot(Pointer entityPointer, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Inserts a single transaction with an atomic commit.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
    Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default);
}
