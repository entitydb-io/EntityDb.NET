using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Manages the snapshots and sources of a single entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface ISingleEntityRepository<TEntity> : IDisposableResource
{
    /// <summary>
    ///     The backing source repository.
    /// </summary>
    ISourceRepository SourceRepository { get; }

    /// <summary>
    ///     The backing snapshot repository (if snapshot is available).
    /// </summary>
    ISnapshotRepository<TEntity>? SnapshotRepository { get; }
    
    /// <summary>
    ///     The pointer for the currently entity.
    /// </summary>
    Pointer EntityPointer { get; }
    
    /// <summary>
    ///     Returns the snapshot of a <typeparamref name="TEntity" />.
    /// </summary>
    /// <returns>The snapshot of a <typeparamref name="TEntity" />.</returns>
    TEntity Get();

    /// <summary>
    ///     Adds a single delta to the source.
    /// </summary>
    /// <param name="delta">The new delta that modifies the <typeparamref name="TEntity" />.</param>
    void Append(object delta);
    
    /// <summary>
    ///     Atomically commits a source.
    /// </summary>
    /// <param name="sourceId">A new id for the new source.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the commit succeeded, or <c>false</c> if the commit failed.</returns>
    Task<bool> Commit(Id sourceId, CancellationToken cancellationToken = default);
}
