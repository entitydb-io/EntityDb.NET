using EntityDb.Abstractions.Tags;

namespace EntityDb.Abstractions.Strategies
{
    /// <summary>
    /// Represents a type used to get tags for a <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be tagged.</typeparam>
    public interface ITaggingStrategy<in TEntity>
    {
        /// <summary>
        /// Returns the tags for a <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The tags for <paramref name="entity"/>.</returns>
        ITag[] GetTags(TEntity entity);
    }
}
