using EntityDb.Abstractions.Tags;

namespace EntityDb.Abstractions.Commands;

/// <summary>
///     Represents a command that adds tags.
/// </summary>
public interface IAddTagsCommand
{
    /// <summary>
    ///     Returns the tags that need to be added.
    /// </summary>
    /// <returns>The tags that need to be added.</returns>
    IEnumerable<ITag> GetTags();
}
