using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Manages the sources and states of a single entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface ISingleEntityRepository<TEntity> : IDisposableResource
{
    /// <summary>
    ///     The backing source repository.
    /// </summary>
    ISourceRepository SourceRepository { get; }

    /// <summary>
    ///     The backing state repository (if state is available).
    /// </summary>
    IStateRepository<TEntity>? StateRepository { get; }

    /// <summary>
    ///     The pointer for the current entity.
    /// </summary>
    Pointer EntityPointer { get; }

    /// <summary>
    ///     Returns the state of a <typeparamref name="TEntity" />.
    /// </summary>
    /// <returns>The state of a <typeparamref name="TEntity" />.</returns>
    TEntity Get();

    /// <summary>
    ///     Adds a single delta to the source.
    /// </summary>
    /// <param name="delta">The new delta that modifies the <typeparamref name="TEntity" />.</param>
    void Append(object delta);

    /// <summary>
    ///     Atomically commits a source.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Only returns <c>false</c> if there are uncommitted messages.</returns>
    Task<bool> Commit(CancellationToken cancellationToken = default);
}
