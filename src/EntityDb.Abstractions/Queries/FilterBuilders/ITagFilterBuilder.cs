using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for a tag query.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface ITagFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes tags with an entity id which is contained in a set of
    ///     entity ids.
    /// </summary>
    /// <param name="entityIds">The set of entity ids.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes tags with an entity id which is contained in
    ///     <paramref name="entityIds" />.
    /// </returns>
    TFilter EntityIdIn(params Id[] entityIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes tags with an entity version number greater than or
    ///     equal to an entity version number.
    /// </summary>
    /// <param name="entityVersionNumber">The entity version number.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes tags with an entity version number greater than or equal
    ///     to <paramref name="entityVersionNumber" />.
    /// </returns>
    TFilter EntityVersionNumberGte(VersionNumber entityVersionNumber);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes tags with an entity version number less than or equal
    ///     to an entity version number.
    /// </summary>
    /// <param name="entityVersionNumber">The entity version number.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes tags with an entity version number less than or equal to
    ///     <paramref name="entityVersionNumber" />.
    /// </returns>
    TFilter EntityVersionNumberLte(VersionNumber entityVersionNumber);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes tags whose type is contained in a set of tag types.
    /// </summary>
    /// <param name="tagTypes">The tag types.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes tags whose type is contained in
    ///     <paramref name="tagTypes" />.
    /// </returns>
    TFilter TagTypeIn(params Type[] tagTypes);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter"/> that only includes tags whose <see cref="ITag.Label"/> is
    ///     a particular value.
    /// </summary>
    /// <param name="label">The tag labels</param>
    /// <returns>
    ///     A <typeparamref name="TFilter"/> that only includes tags whose <see cref="ITag.Label"/> is
    ///     <paramref name="label"/>.
    /// </returns>
    TFilter TagLabelEq(string label);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter"/> that only includes tags whose <see cref="ITag.Value"/> is
    ///     a particular value.
    /// </summary>
    /// <param name="value">The tag values</param>
    /// <returns>
    ///     A <typeparamref name="TFilter"/> that only includes tags whose <see cref="ITag.Value"/> is
    ///     <paramref name="value"/>.
    /// </returns>
    TFilter TagValueEq(string value);
}
