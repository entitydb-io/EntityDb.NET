using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.SortBuilders
{
    /// <summary>
    /// Builds a <typeparamref name="TSort"/> for a fact query.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    public interface IFactSortBuilder<TSort> : ISortBuilder<TSort>
    {
        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders facts by entity id.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders facts by entity id.</returns>
        TSort EntityId(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders facts by entity version number.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders facts by entity version number.</returns>
        TSort EntityVersionNumber(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders facts by entity subversion number.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders facts by entity subversion number.</returns>
        TSort EntitySubversionNumber(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders facts by type.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders facts by type.</returns>
        TSort FactType(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders facts by a fact expression.
        /// </summary>
        /// <typeparam name="TFact">The type of fact in the fact expression.</typeparam>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <param name="factExpression">The fact expression.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders facts by <paramref name="factExpression"/>.</returns>
        TSort FactProperty<TFact>(bool ascending, Expression<Func<TFact, object>> factExpression);
    }
}
