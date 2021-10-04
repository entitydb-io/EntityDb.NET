using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using System;

namespace EntityDb.TestImplementations.Queries
{
    public record TransactionTimeStampQuery(DateTime Gte, DateTime Lte) : ISourceQuery, ICommandQuery, IFactQuery,
        ILeaseQuery, ITagQuery
    {
        public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.TransactionTimeStampGte(Gte),
                builder.TransactionTimeStampLte(Lte)
            );
        }

        public TSort GetSort<TSort>(ICommandSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.TransactionTimeStamp(true),
                builder.EntityId(true),
                builder.EntityVersionNumber(true)
            );
        }

        public TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.TransactionTimeStampGte(Gte),
                builder.TransactionTimeStampLte(Lte)
            );
        }

        public TSort GetSort<TSort>(IFactSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.TransactionTimeStamp(true),
                builder.EntityId(true),
                builder.EntityVersionNumber(true),
                builder.EntitySubversionNumber(true)
            );
        }

        public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.TransactionTimeStampGte(Gte),
                builder.TransactionTimeStampLte(Lte)
            );
        }

        public TSort GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.TransactionTimeStamp(true),
                builder.EntityId(true),
                builder.EntityVersionNumber(true)
            );
        }

        public TFilter GetFilter<TFilter>(ISourceFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.TransactionTimeStampGte(Gte),
                builder.TransactionTimeStampLte(Lte)
            );
        }

        public TSort GetSort<TSort>(ISourceSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.TransactionTimeStamp(true)
            );
        }

        public int? Skip => null;

        public int? Take => null;

        public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.TransactionTimeStampGte(Gte),
                builder.TransactionTimeStampLte(Lte)
            );
        }

        public TSort GetSort<TSort>(ITagSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.TransactionTimeStamp(true),
                builder.EntityId(true),
                builder.EntityVersionNumber(true)
            );
        }
    }
}
