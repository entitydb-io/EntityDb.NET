using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.FilterBuilders
{
    /// <summary>
    /// Builds a <typeparamref name="TFilter"/> for a fact repository.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    public interface IFactFilterBuilder<TFilter> : IFilterBuilder<TFilter>
    {
        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes facts with an entity id which is contained in a set of entity ids.
        /// </summary>
        /// <param name="entityIds">The set of entity ids.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes facts with an entity id which is contained in <paramref name="entityIds"/>.</returns>
        TFilter EntityIdIn(params Guid[] entityIds);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes facts with an entity version number greater than or equal to an entity version number.
        /// </summary>
        /// <param name="entityVersionNumber">The entity version number.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes facts with an entity version number greater than or equal to <paramref name="entityVersionNumber"/>.</returns>
        TFilter EntityVersionNumberGte(ulong entityVersionNumber);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes facts with an entity version number less than or equal to an entity version number.
        /// </summary>
        /// <param name="entityVersionNumber">The entity version number.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes facts with an entity version number less than or equal to <paramref name="entityVersionNumber"/>.</returns>
        TFilter EntityVersionNumberLte(ulong entityVersionNumber);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes facts whose type is contained in a set of fact types.
        /// </summary>
        /// <param name="factTypes">The fact types.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes facts whose type is contained in <paramref name="factTypes"/>.</returns>
        TFilter FactTypeIn(params Type[] factTypes);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes facts which do match a fact expression.
        /// </summary>
        /// <typeparam name="TFact">The type of fact in the fact expression.</typeparam>
        /// <param name="factExpression">The fact expression.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes facts which do match <paramref name="factExpression"/>.</returns>
        TFilter FactMatches<TFact>(Expression<Func<TFact, bool>> factExpression);
    }
}
