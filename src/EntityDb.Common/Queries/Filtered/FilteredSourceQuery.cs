using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Common.Queries.Filtered
{
    internal sealed record FilteredSourceQuery(ISourceQuery SourceQuery, ISourceFilter SourceFilter) : FilteredQueryBase(SourceQuery), ISourceQuery
    {
        public TFilter GetFilter<TFilter>(ISourceFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                SourceQuery.GetFilter(builder),
                SourceFilter.GetFilter(builder)
            );
        }

        public TSort? GetSort<TSort>(ISourceSortBuilder<TSort> builder)
        {
            return SourceQuery.GetSort(builder);
        }
    }
}
