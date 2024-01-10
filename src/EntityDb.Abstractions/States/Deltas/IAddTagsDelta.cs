using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Abstractions.States.Deltas;

/// <summary>
///     Represents a delta that adds tags.
/// </summary>
/// <typeparam name="TState">The type of the state</typeparam>
public interface IAddTagsDelta<in TState>
{
    /// <summary>
    ///     Returns the tags that need to be added.
    /// </summary>
    /// <param name="state">The state for which tags will be added</param>
    /// <returns>The tags that need to be added.</returns>
    IEnumerable<ITag> GetTags(TState state);
}
