using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record GetLastEntityVersionQuery(Id EntityId, object? Options = null) : IMessageQuery
{
    public TFilter GetFilter<TFilter>(IMessageFilterBuilder<TFilter> builder)
    {
        return builder.EntityIdIn(EntityId);
    }

    public TSort GetSort<TSort>(IMessageSortBuilder<TSort> builder)
    {
        return builder.EntityVersion(false);
    }

    public int? Skip => null;

    public int? Take => 1;
}
