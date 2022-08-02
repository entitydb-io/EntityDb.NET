using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Queries;

internal sealed record GetLastEntityCommandQuery(Id EntityId, object? Options = null) : ICommandQuery
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
