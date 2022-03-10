using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Queries;

internal abstract record GetEntityQuery(Id EntityId) : ICommandQuery
{
    protected abstract TFilter GetSubFilter<TFilter>(ICommandFilterBuilder<TFilter> builder);

    public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.EntityIdIn(EntityId),
            GetSubFilter(builder)
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
