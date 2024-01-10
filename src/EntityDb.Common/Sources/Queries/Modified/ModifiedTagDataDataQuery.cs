using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedTagDataDataQuery : ModifiedQueryBase, ITagDataDataQuery
{
    public required ITagDataDataQuery TagDataDataQuery { get; init; }
    protected override IDataQuery DataQuery => TagDataDataQuery;

    public TFilter GetFilter<TFilter>(ITagDataFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                TagDataDataQuery.GetFilter(builder)
            );
        }

        return TagDataDataQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(ITagDataSortBuilder<TSort> builder)
    {
        return TagDataDataQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
