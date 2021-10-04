using EntityDb.Abstractions.Queries.SortBuilders;
using System;
using System.Linq.Expressions;

namespace EntityDb.Common.Queries.SortBuilders
{
    internal sealed record SourceReverseSortBuilder<TSort>(ISourceSortBuilder<TSort> SourceSortBuilder) : ReverseSortBuilderBase<TSort>(SourceSortBuilder), ISourceSortBuilder<TSort>
    {
        public TSort EntityIds(bool ascending)
        {
            return SourceSortBuilder.EntityIds(!ascending);
        }

        public TSort SourceType(bool ascending)
        {
            return SourceSortBuilder.SourceType(!ascending);
        }

        public TSort SourceProperty<TSource>(bool ascending, Expression<Func<TSource, object>> sourceExpression)
        {
            return SourceSortBuilder.SourceProperty(!ascending, sourceExpression);
        }
    }
}
