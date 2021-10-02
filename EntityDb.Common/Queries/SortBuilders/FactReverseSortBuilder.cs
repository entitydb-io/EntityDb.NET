using EntityDb.Abstractions.Queries.SortBuilders;
using System;
using System.Linq.Expressions;

namespace EntityDb.Common.Queries.SortBuilders
{
    internal sealed record FactReverseSortBuilder<TSort>(IFactSortBuilder<TSort> FactSortBuilder) : ReverseSortBuilderBase<TSort>(FactSortBuilder), IFactSortBuilder<TSort>
    {
        public TSort EntityId(bool ascending)
        {
            return FactSortBuilder.EntityId(!ascending);
        }

        public TSort EntityVersionNumber(bool ascending)
        {
            return FactSortBuilder.EntityVersionNumber(!ascending);
        }

        public TSort EntitySubversionNumber(bool ascending)
        {
            return FactSortBuilder.EntitySubversionNumber(!ascending);
        }

        public TSort FactType(bool ascending)
        {
            return FactSortBuilder.FactType(!ascending);
        }

        public TSort FactProperty<TFact>(bool ascending, Expression<Func<TFact, object>> factExpression)
        {
            return FactSortBuilder.FactProperty(!ascending, factExpression);
        }
    }
}
