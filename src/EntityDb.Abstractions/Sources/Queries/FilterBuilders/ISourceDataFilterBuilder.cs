using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Sources.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for an object associated
///     with one or more messages. Possible objects include:
///     agent signatures
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface ISourceDataFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with any state
    ///     id in a set of state ids.
    /// </summary>
    /// <param name="stateIds">The state ids.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with any state
    ///     id in <paramref name="stateIds" />.
    /// </returns>
    TFilter AnyStateIdIn(params Id[] stateIds);
}
