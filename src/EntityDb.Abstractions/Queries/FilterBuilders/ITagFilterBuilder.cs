using System;
using System.Linq.Expressions;

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
    TFilter EntityIdIn(params Guid[] entityIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes tags with an entity version number greater than or
    ///     equal to an entity version number.
    /// </summary>
    /// <param name="entityVersionNumber">The entity version number.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes tags with an entity version number greater than or equal
    ///     to <paramref name="entityVersionNumber" />.
    /// </returns>
    TFilter EntityVersionNumberGte(ulong entityVersionNumber);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes tags with an entity version number less than or equal
    ///     to an entity version number.
    /// </summary>
    /// <param name="entityVersionNumber">The entity version number.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes tags with an entity version number less than or equal to
    ///     <paramref name="entityVersionNumber" />.
    /// </returns>
    TFilter EntityVersionNumberLte(ulong entityVersionNumber);

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
    ///     Returns a <typeparamref name="TFilter" /> that only includes tags which do match a tag expression.
    /// </summary>
    /// <typeparam name="TTag">The type of tag in the tag expression.</typeparam>
    /// <param name="tagExpression">The tag expression.</param>
    /// <returns>A <typeparamref name="TFilter" /> that only includes tags which do match <paramref name="tagExpression" />.</returns>
    TFilter TagMatches<TTag>(Expression<Func<TTag, bool>> tagExpression);
}
