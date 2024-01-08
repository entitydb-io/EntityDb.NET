using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record DeleteLeasesQuery(IReadOnlyCollection<ILease> Leases,
    object? Options = null) : ILeaseQuery
{
    public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
    {
        return builder.Or
        (
            Leases
                .Select(lease => builder.And
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
}
