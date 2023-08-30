using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for a command query.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface ICommandFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes commands with an entity id which is contained in a set
    ///     of entity ids.
    /// </summary>
    /// <param name="entityIds">The set of entity ids.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes commands with an entity id which is contained in
    ///     <paramref name="entityIds" />.
    /// </returns>
    TFilter EntityIdIn(params Id[] entityIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes commands with an entity version number greater than or
    ///     equal to an entity version number.
    /// </summary>
    /// <param name="entityVersionNumber">The entity version number.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes commands with an entity version number greater than or
    ///     equal to <paramref name="entityVersionNumber" />.
    /// </returns>
    TFilter EntityVersionNumberGte(VersionNumber entityVersionNumber);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes commands with an entity version number less than or
    ///     equal to an entity version number.
    /// </summary>
    /// <param name="entityVersionNumber">The entity version number.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes commands with an entity version number less than or equal
    ///     to <paramref name="entityVersionNumber" />.
    /// </returns>
    TFilter EntityVersionNumberLte(VersionNumber entityVersionNumber);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes commands whose type is contained in a set of command
    ///     types.
    /// </summary>
    /// <param name="commandTypes">The command types.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes commands whose type is contained in
    ///     <paramref name="commandTypes" />.
    /// </returns>
    TFilter CommandTypeIn(params Type[] commandTypes);
}
