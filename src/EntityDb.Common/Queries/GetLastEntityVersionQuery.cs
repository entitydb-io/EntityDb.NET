using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using System;

namespace EntityDb.Common.Queries;

internal sealed record GetLastEntityVersionQuery(Guid EntityId) : ICommandQuery
{
    public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
    {
        return builder.EntityIdIn(EntityId);
    }

    public TSort GetSort<TSort>(ICommandSortBuilder<TSort> builder)
    {
        return builder.EntityVersionNumber(false);
    }

    public int? Skip => null;

    public int? Take => 1;
}
