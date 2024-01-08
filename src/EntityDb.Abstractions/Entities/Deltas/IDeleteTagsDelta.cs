using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.Entities.Deltas;

/// <summary>
///     Represents a delta that deletes tags.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IDeleteTagsDelta<in TEntity>
    where TEntity : IEntity<TEntity>
{
    /// <summary>
    ///     Returns the tags that need to be deleted.
    /// </summary>
    /// <param name="entity">The root for which tags will be deleted.</param>
    /// <returns>The tags that need to be deleted.</returns>
    IEnumerable<ITag> GetTags(TEntity entity);
}
