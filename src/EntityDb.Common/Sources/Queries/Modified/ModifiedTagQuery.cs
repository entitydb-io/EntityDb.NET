using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedTagQuery : ModifiedQueryBase, ITagQuery
{
    public required ITagQuery TagQuery { get; init; }
    protected override IQuery Query => TagQuery;

    public TFilter GetFilter<TFilter>(ITagDataFilterBuilder<TFilter> builder)
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
