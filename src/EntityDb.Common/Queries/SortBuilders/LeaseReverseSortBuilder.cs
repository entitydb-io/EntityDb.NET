using EntityDb.Abstractions.Queries.SortBuilders;
using System;
using System.Linq.Expressions;

namespace EntityDb.Common.Queries.SortBuilders
{
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

        public TSort LeaseProperty<TLease>(bool ascending, Expression<Func<TLease, object>> leaseExpression)
        {
            return LeaseSortBuilder.LeaseProperty(!ascending, leaseExpression);
        }
    }
}
