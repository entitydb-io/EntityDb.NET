using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedSourceDataDataQuery : ModifiedQueryBase, ISourceDataDataQuery
{
    public required ISourceDataDataQuery SourceDataDataQuery { get; init; }
    protected override IDataQuery DataQuery => SourceDataDataQuery;

    public TFilter GetFilter<TFilter>(ISourceDataFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                SourceDataDataQuery.GetFilter(builder)
            );
        }

        return SourceDataDataQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(ISourceDataSortBuilder<TSort> builder)
    {
        return SourceDataDataQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
