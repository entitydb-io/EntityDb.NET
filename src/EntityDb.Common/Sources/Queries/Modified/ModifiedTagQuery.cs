using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedTagQuery
    (ITagQuery TagQuery, ModifiedQueryOptions ModifiedQueryOptions) : ModifiedQueryBase(TagQuery,
        ModifiedQueryOptions), ITagQuery
{
    public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                TagQuery.GetFilter(builder)
            );
        }

        return TagQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(ITagSortBuilder<TSort> builder)
    {
        return TagQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
