using EntityDb.Abstractions.States;

namespace EntityDb.Abstractions.Sources.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for an object associated
///     with a single message. Possible objects include:
///     deltas, facts, and tags.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface IMessageDataFilterBuilder<TFilter> : IDataFilterBuilder<TFilter>
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
    /// <param name="stateStateVersion">The state ids.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an state
    ///     version greater than or equal to <paramref name="stateStateVersion" />.
    /// </returns>
    TFilter StateVersionGte(StateVersion stateStateVersion);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an state
    ///     version greater than or equal to a specific state version.
    /// </summary>
    /// <param name="stateStateVersion">The state ids.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an state
    ///     version less than or equal to <paramref name="stateStateVersion" />.
    /// </returns>
    TFilter StateVersionLte(StateVersion stateStateVersion);
}
