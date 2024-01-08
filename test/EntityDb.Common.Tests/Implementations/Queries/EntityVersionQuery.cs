using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Implementations.Queries;

public record EntityVersionQuery(Version Gte, Version Lte, object? Options = null) : IMessageQuery,
    ILeaseQuery, ITagQuery
{
    public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.EntityVersionGte(Gte),
            builder.EntityVersionLte(Lte)
        );
    }

    public TSort GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
    {
        return builder.EntityVersion(true);
    }

    public TFilter GetFilter<TFilter>(IMessageFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.EntityVersionGte(Gte),
            builder.EntityVersionLte(Lte)
        );
    }

    public TSort GetSort<TSort>(IMessageSortBuilder<TSort> builder)
    {
        return builder.EntityVersion(true);
    }

    public int? Skip => null;

    public int? Take => null;

    public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.EntityVersionGte(Gte),
            builder.EntityVersionLte(Lte)
        );
    }

    public TSort GetSort<TSort>(ITagSortBuilder<TSort> builder)
    {
        return builder.EntityVersion(true);
    }
}