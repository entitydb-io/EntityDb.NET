using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Leases;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace EntityDb.Common.Queries
{
    internal sealed record DeleteLeasesQuery(Guid EntityId, ImmutableArray<ILease> Leases) : ILeaseQuery
    {
        public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.EntityIdIn(EntityId),
                builder.Or
                (
                    Leases
                        .Select(deleteLease => builder.LeaseMatches((Lease lease) =>
                            lease.Scope == deleteLease.Scope &&
                            lease.Label == deleteLease.Label &&
                            lease.Value == deleteLease.Value))
                        .ToArray()
                )
            );
        }

        public TSort? GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
        {
            return default;
        }

        public int? Skip => null;

        public int? Take => null;
    }
}
