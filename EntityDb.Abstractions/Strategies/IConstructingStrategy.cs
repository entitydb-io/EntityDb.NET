namespace EntityDb.Abstractions.Strategies
{
    /// <summary>
    /// Represents a type that constructs a new instance of <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be constructed.</typeparam>
    public interface IConstructingStrategy<TEntity>
    {
        /// <summary>
        /// Returns a new instance of a <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>A new instaance of <typeparamref name="TEntity"/>.</returns>
        TEntity Construct();
    }
}
