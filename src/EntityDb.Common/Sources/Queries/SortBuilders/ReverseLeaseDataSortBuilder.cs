using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseLeaseDataSortBuilder<TSort> : ReverseSortBuilderBase<TSort>, ILeaseDataSortBuilder<TSort>
{
    public required ILeaseDataSortBuilder<TSort> LeaseDataSortBuilder { get; init; }
    protected override ISortBuilder<TSort> SortBuilder => LeaseDataSortBuilder;

    public TSort StateId(bool ascending)
    {
        return LeaseDataSortBuilder.StateId(!ascending);
    }

    public TSort StateVersion(bool ascending)
    {
        return LeaseDataSortBuilder.StateVersion(!ascending);
    }

    public TSort LeaseScope(bool ascending)
    {
        return LeaseDataSortBuilder.LeaseScope(!ascending);
    }

    public TSort LeaseLabel(bool ascending)
    {
        return LeaseDataSortBuilder.LeaseLabel(!ascending);
    }

    public TSort LeaseValue(bool ascending)
    {
        return LeaseDataSortBuilder.LeaseValue(!ascending);
    }
}
