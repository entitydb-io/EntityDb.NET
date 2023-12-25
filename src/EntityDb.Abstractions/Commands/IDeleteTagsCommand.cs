using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.ValueObjects;

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
    IEnumerable<ITag> GetTags(Id entityId, VersionNumber entityVersionNumber);

    /// <ignore />
    [Obsolete("Please use GetTags(Id, VersionNumber) instead. This will be removed in a future version,", true)]
    IEnumerable<ITag> GetTags() => throw new NotImplementedException();
}
