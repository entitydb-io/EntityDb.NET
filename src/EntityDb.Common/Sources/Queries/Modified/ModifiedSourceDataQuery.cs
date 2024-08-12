using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedSourceDataQuery : ModifiedQueryBase, ISourceDataQuery
{
    public required ISourceDataQuery SourceDataQuery { get; init; }
    protected override IDataQuery DataQuery => SourceDataQuery;

    public TFilter GetFilter<TFilter>(ISourceDataFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                SourceDataQuery.GetFilter(builder)
            );
        }

        return SourceDataQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(ISourceDataSortBuilder<TSort> builder)
    {
        return SourceDataQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
