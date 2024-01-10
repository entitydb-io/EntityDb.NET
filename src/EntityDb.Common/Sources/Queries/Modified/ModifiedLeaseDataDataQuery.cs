using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedLeaseDataDataQuery : ModifiedQueryBase, ILeaseDataDataQuery
{
    public required ILeaseDataDataQuery LeaseDataDataQuery { get; init; }
    protected override IDataQuery DataQuery => LeaseDataDataQuery;

    public TFilter GetFilter<TFilter>(ILeaseDataFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                LeaseDataDataQuery.GetFilter(builder)
            );
        }

        return LeaseDataDataQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(ILeaseDataSortBuilder<TSort> builder)
    {
        return LeaseDataDataQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
