using EntityDb.Abstractions.Tags;

namespace EntityDb.Abstractions.Commands;

/// <summary>
///     Represents a command that deletes tags.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IDeleteTagsCommand<in TEntity>
{
    /// <summary>
    ///     Returns the tags that need to be deleted.
    /// </summary>
    /// <param name="entity">The entity for which tags will be deleted.</param>
    /// <returns>The tags that need to be deleted.</returns>
    IEnumerable<ITag> GetTags(TEntity entity);
}

/// <ignore />
[Obsolete("Please use IDeleteTagsCommand<TEntity> instead. This will be removed in a future version,", true)]
public interface IDeleteTagsCommand
{
    /// <ignore />
    IEnumerable<ITag> GetTags() => throw new NotImplementedException();
}
