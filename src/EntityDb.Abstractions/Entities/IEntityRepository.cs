using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Encapsulates the source repository and the snapshot repository of an entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IEntityRepository<TEntity> : IDisposableResource
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
    ///     Returns the snapshot of a <typeparamref name="TEntity" /> for a given <see cref="Pointer" />.
    /// </summary>
    /// <param name="entityPointer">A pointer to the entity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The snapshot of a <typeparamref name="TEntity" /> for <paramref name="entityPointer" />.</returns>
    Task<TEntity> GetSnapshot(Pointer entityPointer, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Atomically commits a source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the commit succeeded, or <c>false</c> if the commit failed.</returns>
    Task<bool> Commit(Source source, CancellationToken cancellationToken = default);
}
