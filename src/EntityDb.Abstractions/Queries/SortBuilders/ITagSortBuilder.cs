using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.SortBuilders;

/// <summary>
///     Builds a sort for a tag query.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface ITagSortBuilder<TSort> : ISortBuilder<TSort>
{
    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by entity id.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by entity id.</returns>
    TSort EntityId(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by entity version number.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by entity version number.</returns>
    TSort EntityVersionNumber(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by type.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by type.</returns>
    TSort TagType(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by a tag expression.
    /// </summary>
    /// <typeparam name="TTag">The type of tag in the tag expression.</typeparam>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <param name="tagExpression">The tag expression.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by <paramref name="tagExpression" />.</returns>
    TSort TagProperty<TTag>(bool ascending, Expression<Func<TTag, object>> tagExpression);
}
