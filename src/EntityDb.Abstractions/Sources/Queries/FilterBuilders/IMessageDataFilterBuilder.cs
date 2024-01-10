using EntityDb.Abstractions.ValueObjects;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Abstractions.Sources.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for an object associated
///     with a single message. Possible objects include:
///     deltas, facts, tags, and aliases
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface IMessageDataFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with a state
    ///     id in a set of state ids.
    /// </summary>
    /// <param name="stateIds">The state ids.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with a state
    ///     id in <paramref name="stateIds" />.
    /// </returns>
    TFilter StateIdIn(params Id[] stateIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an state
    ///     version greater than or equal to a specific state version.
    /// </summary>
    /// <param name="stateVersion">The state ids.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an state
    ///     version greater than or equal to <paramref name="stateVersion" />.
    /// </returns>
    TFilter StateVersionGte(Version stateVersion);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an state
    ///     version greater than or equal to a specific state version.
    /// </summary>
    /// <param name="stateVersion">The state ids.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an state
    ///     version less than or equal to <paramref name="stateVersion" />.
    /// </returns>
    TFilter StateVersionLte(Version stateVersion);
}
