using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseLeaseSortBuilder<TSort> : ReverseSortBuilderBase<TSort>, ILeaseSortBuilder<TSort>
{
    public required ILeaseSortBuilder<TSort> LeaseSortBuilder { get; init; }
    protected override ISortBuilder<TSort> SortBuilder => LeaseSortBuilder;

    public TSort StateId(bool ascending)
    {
        return LeaseSortBuilder.StateId(!ascending);
    }

    public TSort StateVersion(bool ascending)
    {
        return LeaseSortBuilder.StateVersion(!ascending);
    }

    public TSort LeaseScope(bool ascending)
    {
        return LeaseSortBuilder.LeaseScope(!ascending);
    }

    public TSort LeaseLabel(bool ascending)
    {
        return LeaseSortBuilder.LeaseLabel(!ascending);
    }

    public TSort LeaseValue(bool ascending)
    {
        return LeaseSortBuilder.LeaseValue(!ascending);
    }
}
