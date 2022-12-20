using EntityDb.Abstractions.Tags;

namespace EntityDb.Abstractions.Commands;

/// <summary>
///     Represents a command that deletes tags.
/// </summary>
public interface IDeleteTagsCommand
{
    /// <summary>
    ///     Returns the tags that need to be deleted.
    /// </summary>
    /// <returns>The tags that need to be deleted.</returns>
    IEnumerable<ITag> GetTags();
}
