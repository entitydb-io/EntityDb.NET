using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.Entities.Deltas;

/// <summary>
///     Represents a delta that adds tags.
/// </summary>
/// <typeparam name="TEntity">The type of the entity</typeparam>
public interface IAddTagsDelta<in TEntity>
    where TEntity : IEntity<TEntity>
{
    /// <summary>
    ///     Returns the tags that need to be added.
    /// </summary>
    /// <param name="entity">The entity for which tags will be added</param>
    /// <returns>The tags that need to be added.</returns>
    IEnumerable<ITag> GetTags(TEntity entity);
}
