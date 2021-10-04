using EntityDb.Abstractions.Queries.FilterBuilders;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    ///     Extensions for filter builders.
    /// </summary>
    public static class IFilterBuilderExtensions
    {
        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do match all filters in a set of filters.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
        /// <param name="filterBuilder">The filter builder.</param>
        /// <param name="filters">The set of filters.</param>
        /// <returns>
        ///     A <typeparamref name="TFilter" /> that excludes objects which do match all filters in
        ///     <paramref name="filters" />.
        /// </returns>
        public static TFilter Nand<TFilter>(this IFilterBuilder<TFilter> filterBuilder, params TFilter[] filters)
        {
            return filterBuilder.Not
            (
                filterBuilder.And(filters)
            );
        }

        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do match any filter in a set of filters.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
        /// <param name="filterBuilder">The filter builder.</param>
        /// <param name="filters">The set of filters.</param>
        /// <returns>
        ///     A <typeparamref name="TFilter" /> that excludes objects which do match any filter in
        ///     <paramref name="filters" />.
        /// </returns>
        public static TFilter Nor<TFilter>(this IFilterBuilder<TFilter> filterBuilder, params TFilter[] filters)
        {
            return filterBuilder.Not
            (
                filterBuilder.Or(filters)
            );
        }

        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> that only includes objects which only match one filter.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
        /// <param name="filterBuilder">The filter builder.</param>
        /// <param name="filterA">The first filter.</param>
        /// <param name="filterB">The second filter.</param>
        /// <returns>
        ///     A <typeparamref name="TFilter" /> that only includes objects which only match <paramref name="filterA" /> or
        ///     only match <paramref name="filterB" />.
        /// </returns>
        public static TFilter Xor<TFilter>(this IFilterBuilder<TFilter> filterBuilder, TFilter filterA, TFilter filterB)
        {
            return filterBuilder.Or
            (
                filterBuilder.And
                (
                    filterA,
                    filterBuilder.Not(filterB)
                ),
                filterBuilder.And
                (
                    filterBuilder.Not(filterA),
                    filterB
                )
            );
        }

        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> that excludes objects which only match one filter.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
        /// <param name="filterBuilder">The filter builder.</param>
        /// <param name="filterA">The first filter.</param>
        /// <param name="filterB">The second filter.</param>
        /// <returns>
        ///     A <typeparamref name="TFilter" /> that excludes objects which only match <paramref name="filterA" /> or only
        ///     match <paramref name="filterB" />.
        /// </returns>
        public static TFilter Xnor<TFilter>(this IFilterBuilder<TFilter> filterBuilder, TFilter filterA,
            TFilter filterB)
        {
            return filterBuilder.Not
            (
                filterBuilder.Xor(filterA, filterB)
            );
        }
    }
}
