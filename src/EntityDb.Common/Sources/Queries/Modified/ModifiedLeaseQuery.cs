using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedLeaseQuery
    (ILeaseQuery LeaseQuery, ModifiedQueryOptions ModifiedQueryOptions) : ModifiedQueryBase(LeaseQuery,
        ModifiedQueryOptions), ILeaseQuery
{
    public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
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
