using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Queries.Filters;

namespace EntityDb.Common.Queries.Filtered
{
    internal sealed record FilteredFactQuery(IFactQuery FactQuery, IFactFilter FactFilter) : FilteredQueryBase(FactQuery), IFactQuery
    {
        public TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                FactQuery.GetFilter(builder),
                FactFilter.GetFilter(builder)
            );
        }

        public TSort? GetSort<TSort>(IFactSortBuilder<TSort> builder)
        {
            return FactQuery.GetSort(builder);
        }
    }
}
