using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseLeaseSortBuilder<TSort>
    (ILeaseSortBuilder<TSort> LeaseSortBuilder) : ReverseSortBuilderBase<TSort>(LeaseSortBuilder),
        ILeaseSortBuilder<TSort>
{
    public TSort EntityId(bool ascending)
    {
        return LeaseSortBuilder.EntityId(!ascending);
    }

    public TSort EntityVersion(bool ascending)
    {
        return LeaseSortBuilder.EntityVersion(!ascending);
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
