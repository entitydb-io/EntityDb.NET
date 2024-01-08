using EntityDb.Abstractions.ValueObjects;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Abstractions.Sources.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for an object associated
///     with a single message. Possible objects include:
///     deltas, facts, tags, and aliases
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface IMessageFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an entity
    ///     branch in a set of entity branches.
    /// </summary>
    /// <param name="entityIds">The entity branches.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an entity
    ///     branch in <paramref name="entityIds" />.
    /// </returns>
    TFilter EntityIdIn(params Id[] entityIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an entity
    ///     version greater than or equal to a specific entity version.
    /// </summary>
    /// <param name="entityVersion">The entity branches.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an entity
    ///     version greater than or equal to <paramref name="entityVersion" />.
    /// </returns>
    TFilter EntityVersionGte(Version entityVersion);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an entity
    ///     version greater than or equal to a specific entity version.
    /// </summary>
    /// <param name="entityVersion">The entity branches.</param>
    /// <returns>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an entity
    ///     version less than or equal to <paramref name="entityVersion" />.
    /// </returns>
    TFilter EntityVersionLte(Version entityVersion);
}
