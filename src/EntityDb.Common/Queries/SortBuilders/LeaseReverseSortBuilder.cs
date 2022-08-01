using EntityDb.Abstractions.Queries.SortBuilders;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

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

    [Obsolete("This method will be removed in the future, and may not be supported for all implementations.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    public TSort LeaseProperty<TLease>(bool ascending, Expression<Func<TLease, object>> leaseExpression)
    {
        return LeaseSortBuilder.LeaseProperty(!ascending, leaseExpression);
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
