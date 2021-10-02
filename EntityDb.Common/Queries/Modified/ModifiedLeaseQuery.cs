using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Queries.Modified
{
    internal sealed record ModifiedLeaseQuery(ILeaseQuery LeaseQuery, bool InvertFilter, bool ReverseSort, int? ReplaceSkip, int? ReplaceTake) : ModifiedQueryBase(LeaseQuery, ReplaceSkip, ReplaceTake), ILeaseQuery
    {
        public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
        {
            if (InvertFilter)
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
            if (ReverseSort)
            {
                return LeaseQuery.GetSort(builder.Reverse());
            }

            return LeaseQuery.GetSort(builder);
        }
    }
}
