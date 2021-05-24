namespace EntityDb.Abstractions.Strategies
{
    /// <summary>
    /// Represents a type used to determine if the next version of an entity should be cached.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity that can be cached.</typeparam>
    public interface ICachingStrategy<TEntity>
    {
        /// <summary>
        /// Determines if the next version of an entity should be cached.
        /// </summary>
        /// <param name="previousEntity">The previous (i.e., already cached) version of the entity, if it exists.</param>
        /// <param name="nextEntity">The next version of the entity.</param>
        /// <returns><c>true</c> if the next version of the entity should be cached, or <c>false</c> if the next version of the entity should not be cached.</returns>
        bool ShouldCache(TEntity? previousEntity, TEntity nextEntity);
    }
}
