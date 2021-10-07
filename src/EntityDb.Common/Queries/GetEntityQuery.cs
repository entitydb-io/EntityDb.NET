using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using System;

namespace EntityDb.Common.Queries
{
    internal sealed record GetEntityQuery(Guid EntityId, ulong StartAfterVersionNumber) : ICommandQuery
    {
        public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.EntityIdIn(EntityId),
                builder.EntityVersionNumberGte(StartAfterVersionNumber + 1)
            );
        }

        public TSort GetSort<TSort>(ICommandSortBuilder<TSort> builder)
        {
            return builder.Combine
            (
                builder.EntityVersionNumber(true)
            );
        }

        public int? Skip => null;

        public int? Take => null;
    }
}
