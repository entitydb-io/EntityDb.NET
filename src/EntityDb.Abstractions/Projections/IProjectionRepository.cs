using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
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
    ///     The backing transaction repository.
    /// </summary>
    ITransactionRepository TransactionRepository { get; }

    /// <summary>
    ///     The backing snapshot repository.
    /// </summary>
    ISnapshotRepository<TProjection>? SnapshotRepository { get; }

    /// <ignore />
    [Obsolete("Please use GetSnapshot(...) instead. This method will be removed at a later date.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    public Task<TProjection> GetCurrent(Id projectionId, CancellationToken cancellationToken = default)
        => GetSnapshot(projectionId, cancellationToken);

    /// <summary>
    ///     Returns the snapshot of a <typeparamref name="TProjection" /> for a given <see cref="Pointer"/>.
    /// </summary>
    /// <param name="projectionPointer">A pointer to the projection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The snapshot of a <typeparamref name="TProjection" /> for <paramref name="projectionPointer"/>.</returns>
    Task<TProjection> GetSnapshot(Pointer projectionPointer, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Maps an entity to a projection id, or default if the entity does not map to this projection.
    /// </summary>
    /// <param name="entity">The entity object.</param>
    /// <returns>The projection id for the entity, or default if none.</returns>
    Id? GetProjectionIdOrDefault(object entity);
}
