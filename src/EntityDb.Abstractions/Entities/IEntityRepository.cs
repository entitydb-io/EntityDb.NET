using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

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
    ///     Returns the current state of a <typeparamref name="TEntity" /> or constructs a new <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The current state of a <typeparamref name="TEntity" /> or constructs a new <typeparamref name="TEntity" />.</returns>
    Task<TEntity> GetCurrent(Id entityId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a previous state of <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <param name="lteVersionNumber">The version of the entity to fetch.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A previous state of <typeparamref name="TEntity" />.</returns>
    Task<TEntity> GetAtVersion(Id entityId, VersionNumber lteVersionNumber, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Inserts a single transaction with an atomic commit.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
    Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default);
}
