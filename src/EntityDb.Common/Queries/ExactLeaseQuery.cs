using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Leases;

namespace EntityDb.Common.Queries
{
    /// <summary>
    /// A query for the exact lease.
    /// </summary>
    public sealed record ExactLeaseQuery(ILease Lease) : ILeaseQuery
    {
        /// <inheritdoc cref="ILeaseQuery.GetFilter{TFilter}(ILeaseFilterBuilder{TFilter})" />
        public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
        {
            return builder.LeaseMatches<Lease>((x) =>
                x.Scope == Lease.Scope &&
                x.Label == Lease.Label &&
                x.Value == Lease.Value);
        }

        /// <inheritdoc cref="ILeaseQuery.GetSort{TSort}(ILeaseSortBuilder{TSort})" />
        public TSort? GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
        {
            return default;
        }

        /// <inheritdoc cref="IQuery.Skip" />
        public int? Skip => null;

        /// <inheritdoc cref="IQuery.Take" />
        public int? Take => 1;
    }
}
