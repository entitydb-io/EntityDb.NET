namespace EntityDb.Abstractions.Commands;

/// <summary>
///     Represents the intent to modify a <typeparamref name="TEntity" />.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be modified.</typeparam>
public interface ICommand<TEntity>
{
    /// <summary>
    ///     Returns a new <typeparamref name="TEntity" /> that incorporates the modification into an entity.
    /// </summary>
    /// <param name="entity">The entity to be modified.</param>
    /// <returns>
    ///     A new <typeparamref name="TEntity" /> that incorporates the modification of this command into
    ///     <paramref name="entity" />.
    /// </returns>
    TEntity Reduce(TEntity entity);
}
