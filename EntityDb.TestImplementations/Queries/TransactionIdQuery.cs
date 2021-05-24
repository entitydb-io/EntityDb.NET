using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using System;

namespace EntityDb.TestImplementations.Queries
{
    public record TransactionIdQuery(Guid TransactionId) : ISourceQuery, ICommandQuery, IFactQuery, ITagQuery
    {
        public TFilter GetFilter<TFilter>(ISourceFilterBuilder<TFilter> builder)
        {
            return builder.TransactionIdIn(TransactionId);
        }

        public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            return builder.TransactionIdIn(TransactionId);
        }

        public TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder)
        {
            return builder.TransactionIdIn(TransactionId);
        }

        public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
        {
            return builder.TransactionIdIn(TransactionId);
        }

        public TSort? GetSort<TSort>(ISourceSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.TransactionId(true)
            );
        }

        public TSort? GetSort<TSort>(ICommandSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.TransactionId(true),
                builder.EntityId(true),
                builder.EntityVersionNumber(true)
            );
        }

        public TSort? GetSort<TSort>(IFactSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.TransactionId(true),
                builder.EntityId(true),
                builder.EntityVersionNumber(true),
                builder.EntitySubversionNumber(true)
            );
        }

        public TSort? GetSort<TSort>(ITagSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.TransactionId(true),
                builder.EntityId(true),
                builder.EntityVersionNumber(true)
            );
        }

        public int? Skip => null;

        public int? Take => null;
    }
}
