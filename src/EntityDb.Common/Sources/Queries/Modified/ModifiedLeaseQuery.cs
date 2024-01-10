using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedLeaseQuery : ModifiedQueryBase, ILeaseQuery
{
    public required ILeaseQuery LeaseQuery { get; init; }
    protected override IQuery Query => LeaseQuery;

    public TFilter GetFilter<TFilter>(ILeaseDataFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                LeaseQuery.GetFilter(builder)
            );
        }

        return LeaseQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
    {
        return LeaseQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
