using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Projections;

/// <summary>
///     Encapsulates the snapshot repository for a projection.
/// </summary>
/// <typeparam name="TProjection">The type of the projection.</typeparam>
public interface IProjectionRepository<TProjection> : IDisposableResource
{
    /// <summary>
    ///     The strategy for mapping between projection id and entity id.
    /// </summary>
    IProjectionStrategy<TProjection> ProjectionStrategy { get; }

    /// <summary>
    ///     The backing transaction repository.
    /// </summary>
    ITransactionRepository TransactionRepository { get; }
    
    /// <summary>
    ///     The backing snapshot repository.
    /// </summary>
    ISnapshotRepository<TProjection>? SnapshotRepository { get; }

    /// <summary>
    ///     Returns the current state of a <typeparamref name="TProjection" />.
    /// </summary>
    /// <param name="projectionId">The id of the projection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The current state of a <typeparamref name="TProjection" />.</returns>
    Task<TProjection> GetCurrent(Id projectionId, CancellationToken cancellationToken = default);
}
