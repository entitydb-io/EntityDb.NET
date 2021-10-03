using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Queries.Modified
{
    internal sealed record ModifiedLeaseQuery(ILeaseQuery LeaseQuery, ModifiedQueryOptions ModifiedQueryOptions) : ModifiedQueryBase(LeaseQuery, ModifiedQueryOptions), ILeaseQuery
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
            if (ModifiedQueryOptions.ReverseSort)
            {
                return LeaseQuery.GetSort(builder.Reverse());
            }

            return LeaseQuery.GetSort(builder);
        }
    }
}
