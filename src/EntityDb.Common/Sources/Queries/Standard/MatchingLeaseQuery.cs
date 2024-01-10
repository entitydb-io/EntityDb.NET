using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record MatchingLeaseQuery(ILease Lease) : ILeaseQuery
{
    public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.LeaseScopeEq(Lease.Scope),
            builder.LeaseLabelEq(Lease.Label),
            builder.LeaseValueEq(Lease.Value)
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
