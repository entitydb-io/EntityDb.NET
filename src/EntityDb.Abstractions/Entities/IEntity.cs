using EntityDb.Abstractions.States;

namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Indicates the entity is compatible with several EntityDb.Common implementations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IEntity<TEntity> : IState<TEntity>
{
    /// <summary>
    ///     Returns <c>true</c> if <see cref="Reduce(object)" /> is not expected to throw an exception.
    /// </summary>
    /// <param name="delta">The delta</param>
    /// <returns><c>true</c> if <see cref="Reduce(object)" /> is not expected to throw an exception.</returns>
    static abstract bool CanReduce(object delta);

    /// <summary>
    ///     Returns a new <typeparamref name="TEntity" /> that incorporates the deltas.
    /// </summary>
    /// <param name="delta">The delta</param>
    /// <returns>A new <typeparamref name="TEntity" /> that incorporates <paramref name="delta" />.</returns>
    TEntity Reduce(object delta);
}
