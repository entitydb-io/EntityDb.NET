using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record MatchingLeasesQuery(params ILease[] Leases) : ILeaseQuery
{
    public TFilter GetFilter<TFilter>(ILeaseDataFilterBuilder<TFilter> builder)
    {
        return builder.Or
        (
            Leases.Select(lease => builder.And
                (
                    builder.LeaseScopeEq(lease.Scope),
                    builder.LeaseLabelEq(lease.Label),
                    builder.LeaseValueEq(lease.Value)
                ))
                .ToArray()
        );
    }

    public TSort? GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
    {
        return default;
    }

    public int? Skip => null;

    public int? Take => null;

    public object? Options => null;
}
