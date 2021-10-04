using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Queries.Filters;

namespace EntityDb.Common.Queries.Filtered
{
    internal sealed record FilteredTagQuery(ITagQuery TagQuery, ITagFilter TagFilter) : FilteredQueryBase(TagQuery),
        ITagQuery
    {
        public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                TagQuery.GetFilter(builder),
                TagFilter.GetFilter(builder)
            );
        }

        public TSort? GetSort<TSort>(ITagSortBuilder<TSort> builder)
        {
            return TagQuery.GetSort(builder);
        }
    }
}
