using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Projections;

/// <summary>
///     Encapsulates the snapshot repository for a projection.
/// </summary>
/// <typeparam name="TProjection">The type of the projection.</typeparam>
public interface IProjectionRepository<TProjection> : ISourceRepository, IDisposableResource
{
    /// <summary>
    ///     The backing snapshot repository.
    /// </summary>
    ISnapshotRepository<TProjection>? SnapshotRepository { get; }

    /// <summary>
    ///     Returns the snapshot of a <typeparamref name="TProjection" /> for a given <see cref="Pointer" />.
    /// </summary>
    /// <param name="projectionPointer">A pointer to the projection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The snapshot of a <typeparamref name="TProjection" /> for <paramref name="projectionPointer" />.</returns>
    Task<TProjection> GetSnapshot(Pointer projectionPointer, CancellationToken cancellationToken = default);
}
