using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.FilterBuilders
{
    /// <summary>
    /// Builds a <typeparamref name="TFilter"/> for a command query.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    public interface ICommandFilterBuilder<TFilter> : IFilterBuilder<TFilter>
    {
        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes commands with an entity id which is contained in a set of entity ids.
        /// </summary>
        /// <param name="entityIds">The set of entity ids.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes commands with an entity id which is contained in <paramref name="entityIds"/>.</returns>
        TFilter EntityIdIn(params Guid[] entityIds);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes commands with an entity version number greater than or equal to an entity version number.
        /// </summary>
        /// <param name="entityVersionNumber">The entity version number.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes commands with an entity version number greater than or equal to <paramref name="entityVersionNumber"/>.</returns>
        TFilter EntityVersionNumberGte(ulong entityVersionNumber);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes commands with an entity version number less than or equal to an entity version number.
        /// </summary>
        /// <param name="entityVersionNumber">The entity version number.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes commands with an entity version number less than or equal to <paramref name="entityVersionNumber"/>.</returns>
        TFilter EntityVersionNumberLte(ulong entityVersionNumber);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes commands whose type is contained in a set of command types.
        /// </summary>
        /// <param name="commandTypes">The command types.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes commands whose type is contained in <paramref name="commandTypes"/>.</returns>
        TFilter CommandTypeIn(params Type[] commandTypes);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes commands which do match a command expression.
        /// </summary>
        /// <typeparam name="TCommand">The type of command in the command expression.</typeparam>
        /// <param name="commandExpression">The command expression.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes commands which do match <paramref name="commandExpression"/>.</returns>
        TFilter CommandMatches<TCommand>(Expression<Func<TCommand, bool>> commandExpression);
    }
}
