using EntityDb.Abstractions.Tags;

namespace EntityDb.Abstractions.Commands;

/// <summary>
///     Represents a command that adds tags.
/// </summary>
/// <typeparam name="TEntity">The type of the entity</typeparam>
public interface IAddTagsCommand<in TEntity>
{
    /// <summary>
    ///     Returns the tags that need to be added.
    /// </summary>
    /// <param name="entity">The entity for which tags will be added</param>
    /// <returns>The tags that need to be added.</returns>
    IEnumerable<ITag> GetTags(TEntity entity);
}

/// <ignore />
[Obsolete("Please use IAddTagsCommand<TEntity> instead. This will be removed in a future version.", true)]
public interface IAddTagsCommand
{
    /// <ignore />
    IEnumerable<ITag> GetTags() => throw new NotImplementedException();
}
