using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedLeaseDataQuery : ModifiedQueryBase, ILeaseDataQuery
{
    public required ILeaseDataQuery LeaseDataQuery { get; init; }
    protected override IDataQuery DataQuery => LeaseDataQuery;

    public TFilter GetFilter<TFilter>(ILeaseDataFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                LeaseDataQuery.GetFilter(builder)
            );
        }

        return LeaseDataQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(ILeaseDataSortBuilder<TSort> builder)
    {
        return LeaseDataQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
