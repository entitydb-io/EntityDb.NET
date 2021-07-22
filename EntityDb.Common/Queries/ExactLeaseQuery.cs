using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.Leases;
using EntityDb.Common.Leases;

namespace EntityDb.Common.Queries
{
    /// <summary>
    /// A query for the exact lease.
    /// </summary>
    public sealed record ExactLeaseQuery(ILease Lease) : ILeaseQuery
    {
        public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
        {
            return builder.LeaseMatches<Lease>((x) =>
                x.Scope == Lease.Scope &&
                x.Label == Lease.Label &&
                x.Value == Lease.Value);
        }

        public TSort? GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
        {
            return default;
        }

        public int? Skip => null;

        public int? Take => 1;
    }
}
