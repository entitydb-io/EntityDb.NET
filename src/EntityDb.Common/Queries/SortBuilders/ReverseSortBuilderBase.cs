using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Common.Queries.SortBuilders
{
    internal abstract record ReverseSortBuilderBase<TSort>(ISortBuilder<TSort> SortBuilder)
    {
        public TSort TransactionTimeStamp(bool ascending)
        {
            return SortBuilder.TransactionTimeStamp(!ascending);
        }

        public TSort TransactionId(bool ascending)
        {
            return SortBuilder.TransactionId(!ascending);
        }

        public TSort Combine(params TSort[] sorts)
        {
            return SortBuilder.Combine(sorts);
        }
    }
}
