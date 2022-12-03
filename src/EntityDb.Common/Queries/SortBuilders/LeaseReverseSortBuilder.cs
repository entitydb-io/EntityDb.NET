using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Common.Queries.SortBuilders;

internal sealed record LeaseReverseSortBuilder<TSort>
    (ILeaseSortBuilder<TSort> LeaseSortBuilder) : ReverseSortBuilderBase<TSort>(LeaseSortBuilder),
        ILeaseSortBuilder<TSort>
{
    public TSort EntityId(bool ascending)
    {
        return LeaseSortBuilder.EntityId(!ascending);
    }

    public TSort EntityVersionNumber(bool ascending)
    {
        return LeaseSortBuilder.EntityVersionNumber(!ascending);
    }

    public TSort LeaseType(bool ascending)
    {
        return LeaseSortBuilder.LeaseType(!ascending);
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
