using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.SortBuilders
{
    /// <summary>
    ///     Builds a <typeparamref name="TSort" /> for a source query.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    public interface ISourceSortBuilder<TSort> : ISortBuilder<TSort>
    {
        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> that orders sources by entity ids.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort" /> that orders sources by entity ids.</returns>
        TSort EntityIds(bool ascending);

        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> that orders sources by type.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort" /> that orders sources by type.</returns>
        TSort SourceType(bool ascending);

        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> that orders sources by a source expression.
        /// </summary>
        /// <typeparam name="TSource">The type of source in the source expression.</typeparam>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <param name="sourceExpression">The source expression.</param>
        /// <returns>A <typeparamref name="TSort" /> that orders sources by <paramref name="sourceExpression" />.</returns>
        TSort SourceProperty<TSource>(bool ascending, Expression<Func<TSource, object>> sourceExpression);
    }
}
