using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Sources.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for an object associated
///     with one or more messages. Possible objects include:
///     agent signatures
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface IMessageGroupFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with any entity
    ///     id in a set of entity ids.
    /// </summary>
    /// <param name="entityIds">The entity ids.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with any entity
    ///     id in <paramref name="entityIds" />.
    /// </returns>
    TFilter AnyEntityIdIn(params Id[] entityIds);
}
