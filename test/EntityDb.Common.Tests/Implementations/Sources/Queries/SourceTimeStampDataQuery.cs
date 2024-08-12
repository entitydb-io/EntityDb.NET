using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Tests.Implementations.Sources.Queries;

public sealed record SourceTimeStampDataQuery(TimeStamp Gte, TimeStamp Lte, object? Options = null) : ISourceDataQuery,
    IMessageDataQuery,
    ILeaseDataQuery, ITagDataQuery
{
    public TFilter GetFilter<TFilter>(ILeaseDataFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.SourceTimeStampGte(Gte),
            builder.SourceTimeStampLte(Lte)
        );
    }

    public TSort GetSort<TSort>(ILeaseDataSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.SourceTimeStamp(true),
            builder.StateId(true),
            builder.StateVersion(true)
        );
    }

    public TFilter GetFilter<TFilter>(IMessageDataFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.SourceTimeStampGte(Gte),
            builder.SourceTimeStampLte(Lte)
        );
    }

    public TSort GetSort<TSort>(IMessageDataSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.SourceTimeStamp(true),
            builder.StateId(true),
            builder.StateVersion(true)
        );
    }

    public TFilter GetFilter<TFilter>(ISourceDataFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.SourceTimeStampGte(Gte),
            builder.SourceTimeStampLte(Lte)
        );
    }

    public TSort GetSort<TSort>(ISourceDataSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.SourceTimeStamp(true)
        );
    }

    public int? Skip => null;

    public int? Take => null;

    public TFilter GetFilter<TFilter>(ITagDataFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.SourceTimeStampGte(Gte),
            builder.SourceTimeStampLte(Lte)
        );
    }

    public TSort GetSort<TSort>(ITagDataSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.SourceTimeStamp(true),
            builder.StateId(true),
            builder.StateVersion(true)
        );
    }
}
