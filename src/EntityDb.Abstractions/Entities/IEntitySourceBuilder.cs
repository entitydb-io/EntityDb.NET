using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Provides a way to construct an <see cref="Source" />. Note that no operations are permanent
///     until you call <see cref="Build" /> and pass the result to a source repository.
/// </summary>
/// <typeparam name="TEntity">The type of the entity in the source.</typeparam>
public interface IEntitySourceBuilder<TEntity>
{
    /// <summary>
    ///     Returns a <typeparamref name="TEntity" /> associated with a given branch, if it is known.
    /// </summary>
    /// <param name="entityId">The id associated with the entity.</param>
    /// <returns>A <typeparamref name="TEntity" /> associated with <paramref name="entityId" />, if it is known.</returns>
    TEntity GetEntity(Id entityId);

    /// <summary>
    ///     Indicates whether or not a <typeparamref name="TEntity" /> associated with a given branch is in memory.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <returns>
    ///     <c>true</c> if a <typeparamref name="TEntity" /> associated with <paramref name="entityId" /> is in memory, or
    ///     else <c>false</c>.
    /// </returns>
    bool IsEntityKnown(Id entityId);

    /// <summary>
    ///     Associate a <typeparamref name="TEntity" /> with a given entity branch.
    /// </summary>
    /// <param name="entityId">A branch associated with a <typeparamref name="TEntity" />.</param>
    /// <param name="entity">A <typeparamref name="TEntity" />.</param>
    /// <returns>The source builder.</returns>
    /// <remarks>
    ///     Call this method to load an entity that already exists before calling
    ///     <see cref="Append" />.
    /// </remarks>
    IEntitySourceBuilder<TEntity> Load(Id entityId, TEntity entity);

    /// <summary>
    ///     Adds a single delta to the source with a given entity branch.
    /// </summary>
    /// <param name="entityId">The branch associated with the <typeparamref name="TEntity" />.</param>
    /// <param name="delta">The new delta that modifies the <typeparamref name="TEntity" />.</param>
    /// <returns>The source builder.</returns>
    IEntitySourceBuilder<TEntity> Append(Id entityId, object delta);

    /// <summary>
    ///     Returns a new instance of <see cref="Source" />.
    /// </summary>
    /// <param name="sourceId">A new id for the new source.</param>
    /// <returns>A new instance of <see cref="Source" />.</returns>
    Source Build(Id sourceId);
}
