using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Manages the sources and snapshots of multiple entities.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IMultipleEntityRepository<TEntity> : IDisposableResource
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
    ///     Start a new entity if a given entity id.
    /// </summary>
    /// <param name="entityId">A new id for the new entity.</param>
    /// <remarks>
    ///     Only call this method for entities that do not already exist.
    /// </remarks>
    void Create(Id entityId);

    /// <summary>
    ///     Associate a <typeparamref name="TEntity" /> with a given entity id.
    /// </summary>
    /// <param name="entityPointer">A pointer associated with a <typeparamref name="TEntity" />.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task.</returns>
    /// <remarks>
    ///     Call this method to load an entity that already exists before calling
    ///     <see cref="Append" />.
    /// </remarks>
    Task Load(Pointer entityPointer, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     Returns the snapshot of a <typeparamref name="TEntity" /> for a given <see cref="Id" />.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <returns>The snapshot of a <typeparamref name="TEntity" /> for <paramref name="entityId" />.</returns>
    TEntity Get(Id entityId);

    /// <summary>
    ///     Adds a single delta to the source with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity" />.</param>
    /// <param name="delta">The new delta that modifies the <typeparamref name="TEntity" />.</param>
    void Append(Id entityId, object delta);
    
    /// <summary>
    ///     Atomically commits a source.
    /// </summary>
    /// <param name="sourceId">A new id for the new source.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the commit succeeded, or <c>false</c> if the commit failed.</returns>
    Task<bool> Commit(Id sourceId, CancellationToken cancellationToken = default);
}
