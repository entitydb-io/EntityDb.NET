using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Queries;

internal sealed record DeleteLeasesQuery(Id EntityId, IReadOnlyCollection<ILease> Leases, object? Options = null) : ILeaseQuery
{
    public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.EntityIdIn(EntityId),
            builder.Or
            (
                Leases
                    .Select(deleteLease => builder.And(
                            builder.LeaseScopeEq(deleteLease.Scope),
                            builder.LeaseLabelEq(deleteLease.Label),
                            builder.LeaseValueEq(deleteLease.Value)
                        ))
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
