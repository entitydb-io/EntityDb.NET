using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedTagDataQuery : ModifiedQueryBase, ITagDataQuery
{
    public required ITagDataQuery TagDataQuery { get; init; }
    protected override IDataQuery DataQuery => TagDataQuery;

    public TFilter GetFilter<TFilter>(ITagDataFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                TagDataQuery.GetFilter(builder)
            );
        }

        return TagDataQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(ITagDataSortBuilder<TSort> builder)
    {
        return TagDataQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
