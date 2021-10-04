namespace EntityDb.Abstractions.Facts
{
    /// <summary>
    ///     Represents a modifier for a <typeparamref name="TEntity" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be modified.</typeparam>
    public interface IFact<TEntity>
    {
        /// <summary>
        ///     Returns a new <typeparamref name="TEntity" /> that incorporates the modification of this fact into an entity.
        /// </summary>
        /// <param name="entity">The entity to be modified.</param>
        /// <returns>
        ///     A new <typeparamref name="TEntity" /> that incorporates the modification of this fact into
        ///     <paramref name="entity" />.
        /// </returns>
        TEntity Reduce(TEntity entity);
    }
}
