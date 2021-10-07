namespace EntityDb.Abstractions.Strategies
{
    /// <summary>
    ///     Represents a type used to manage versioning for a <typeparamref name="TEntity" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be versioned.</typeparam>
    public interface IVersioningStrategy<TEntity>
    {
        /// <summary>
        ///     Returns the version number of a <typeparamref name="TEntity" />.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The version number of <paramref name="entity" />.</returns>
        ulong GetVersionNumber(TEntity entity);
    }
}
