using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.FilterBuilders
{
    /// <summary>
    /// Builds a <typeparamref name="TFilter"/> for a source query.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    public interface ISourceFilterBuilder<TFilter> : IFilterBuilder<TFilter>
    {
        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes sources with any entity id which is contained in a set of entity ids.
        /// </summary>
        /// <param name="entityIds">The set of entity ids.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes sources with any entity id which is contained in <paramref name="entityIds"/>.</returns>
        TFilter EntityIdsIn(params Guid[] entityIds);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes sources whose type is contained in a set of source types.
        /// </summary>
        /// <param name="sourceTypes">The source types.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes sources whose type is contained in <paramref name="sourceTypes"/>.</returns>
        TFilter SourceTypeIn(params Type[] sourceTypes);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes sources which do match a source expression.
        /// </summary>
        /// <typeparam name="TSource">The type of source in the source expression.</typeparam>
        /// <param name="sourceExpression">The source expression.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes sources which do match <paramref name="sourceExpression"/>.</returns>
        TFilter SourceMatches<TSource>(Expression<Func<TSource, bool>> sourceExpression);
    }
}
