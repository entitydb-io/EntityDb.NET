using EntityDb.Abstractions.Sources.Queries.FilterBuilders;

namespace EntityDb.Common.Sources.Queries.FilterBuilders;

/// <summary>
///     Extensions for filter builders.
/// </summary>
public static class FilterBuilderExtensions
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do match all filters in a set of filters.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    /// <param name="dataFilterBuilder">The filter builder.</param>
    /// <param name="filters">The set of filters.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that excludes objects which do match all filters in
    ///     <paramref name="filters" />.
    /// </returns>
    public static TFilter Nand<TFilter>(this IDataFilterBuilder<TFilter> dataFilterBuilder,
        params TFilter[] filters)
    {
        return dataFilterBuilder.Not
        (
            dataFilterBuilder.And(filters)
        );
    }

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do match any filter in a set of filters.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    /// <param name="dataFilterBuilder">The filter builder.</param>
    /// <param name="filters">The set of filters.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that excludes objects which do match any filter in
    ///     <paramref name="filters" />.
    /// </returns>
    public static TFilter Nor<TFilter>(this IDataFilterBuilder<TFilter> dataFilterBuilder, params TFilter[] filters)
    {
        return dataFilterBuilder.Not
        (
            dataFilterBuilder.Or(filters)
        );
    }

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects which only match one filter.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    /// <param name="dataFilterBuilder">The filter builder.</param>
    /// <param name="filterA">The first filter.</param>
    /// <param name="filterB">The second filter.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes objects which only match <paramref name="filterA" /> or
    ///     only match <paramref name="filterB" />.
    /// </returns>
    public static TFilter Xor<TFilter>(this IDataFilterBuilder<TFilter> dataFilterBuilder, TFilter filterA,
        TFilter filterB)
    {
        return dataFilterBuilder.Or
        (
            dataFilterBuilder.And
            (
                filterA,
                dataFilterBuilder.Not(filterB)
            ),
            dataFilterBuilder.And
            (
                dataFilterBuilder.Not(filterA),
                filterB
            )
        );
    }

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that excludes objects which only match one filter.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    /// <param name="dataFilterBuilder">The filter builder.</param>
    /// <param name="filterA">The first filter.</param>
    /// <param name="filterB">The second filter.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that excludes objects which only match <paramref name="filterA" /> or only
    ///     match <paramref name="filterB" />.
    /// </returns>
    public static TFilter Xnor<TFilter>(this IDataFilterBuilder<TFilter> dataFilterBuilder, TFilter filterA,
        TFilter filterB)
    {
        return dataFilterBuilder.Not
        (
            dataFilterBuilder.Xor(filterA, filterB)
        );
    }
}
