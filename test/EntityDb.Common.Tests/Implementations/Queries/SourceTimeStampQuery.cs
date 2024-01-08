using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Tests.Implementations.Queries;

public record SourceTimeStampQuery(TimeStamp Gte, TimeStamp Lte, object? Options = null) : IMessageGroupQuery,
    IMessageQuery,
    ILeaseQuery, ITagQuery
{
    public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.SourceTimeStampGte(Gte),
            builder.SourceTimeStampLte(Lte)
        );
    }

    public TSort GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.SourceTimeStamp(true),
            builder.EntityId(true),
            builder.EntityVersion(true)
        );
    }

    public TFilter GetFilter<TFilter>(IMessageGroupFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.SourceTimeStampGte(Gte),
            builder.SourceTimeStampLte(Lte)
        );
    }

    public TSort GetSort<TSort>(IMessageGroupSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.SourceTimeStamp(true)
        );
    }

    public int? Skip => null;

    public int? Take => null;

    public TFilter GetFilter<TFilter>(IMessageFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.SourceTimeStampGte(Gte),
            builder.SourceTimeStampLte(Lte)
        );
    }

    public TSort GetSort<TSort>(IMessageSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.SourceTimeStamp(true),
            builder.EntityId(true),
            builder.EntityVersion(true)
        );
    }

    public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.SourceTimeStampGte(Gte),
            builder.SourceTimeStampLte(Lte)
        );
    }

    public TSort GetSort<TSort>(ITagSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.SourceTimeStamp(true),
            builder.EntityId(true),
            builder.EntityVersion(true)
        );
    }
}