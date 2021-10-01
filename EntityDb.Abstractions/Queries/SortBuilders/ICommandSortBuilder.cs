using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.SortBuilders
{
    /// <summary>
    /// Builds a <typeparamref name="TSort"/> for a command query.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    public interface ICommandSortBuilder<TSort> : ISortBuilder<TSort>
    {
        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders commands by entity id.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders commands by entity id.</returns>
        TSort EntityId(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders commands by entity version number.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders commands by entity version number.</returns>
        TSort EntityVersionNumber(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders commands by type.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders commands by type.</returns>
        TSort CommandType(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders commands by a command expression.
        /// </summary>
        /// <typeparam name="TCommand">The type of command in the command expression.</typeparam>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <param name="commandExpression">The command expression.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders commands by <paramref name="commandExpression"/>.</returns>
        TSort CommandProperty<TCommand>(bool ascending, Expression<Func<TCommand, object>> commandExpression);
    }
}
