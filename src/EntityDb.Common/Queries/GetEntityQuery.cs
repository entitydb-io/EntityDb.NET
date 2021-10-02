using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using System;

namespace EntityDb.Common.Queries
{
    internal sealed record GetEntityQuery(Guid EntityId, ulong StartAfterVersionNumber) : IFactQuery
    {
        public TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.EntityIdIn(EntityId),
                builder.EntityVersionNumberGte(StartAfterVersionNumber + 1)
            );
        }

        public TSort? GetSort<TSort>(IFactSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.EntityVersionNumber(true),
                builder.EntitySubversionNumber(true)
            );
        }

        public int? Skip => null;

        public int? Take => null;
    }
}
