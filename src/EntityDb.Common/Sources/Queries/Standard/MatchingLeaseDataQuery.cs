using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record MatchingLeaseDataQuery(ILease Lease) : ILeaseDataDataQuery
{
    public TFilter GetFilter<TFilter>(ILeaseDataFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.LeaseScopeEq(Lease.Scope),
            builder.LeaseLabelEq(Lease.Label),
            builder.LeaseValueEq(Lease.Value)
        );
    }

    public TSort? GetSort<TSort>(ILeaseDataSortBuilder<TSort> builder)
    {
        return default;
    }

    public int? Skip => null;

    public int? Take => null;

    public object? Options => null;
}
