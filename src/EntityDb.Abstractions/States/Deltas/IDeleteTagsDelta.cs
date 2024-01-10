using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Abstractions.States.Deltas;

/// <summary>
///     Represents a delta that deletes tags.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public interface IDeleteTagsDelta<in TState>
{
    /// <summary>
    ///     Returns the tags that need to be deleted.
    /// </summary>
    /// <param name="state">The state for which tags will be deleted.</param>
    /// <returns>The tags that need to be deleted.</returns>
    IEnumerable<ITag> GetTags(TState state);
}
