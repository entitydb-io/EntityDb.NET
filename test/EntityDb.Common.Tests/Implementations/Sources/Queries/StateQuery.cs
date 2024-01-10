﻿using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Tests.Implementations.Sources.Queries;

public record StateQuery(Id StateId, object? Options = null) : ISourceDataQuery, IMessageDataQuery,
    ILeaseQuery, ITagQuery
{
    public TFilter GetFilter<TFilter>(ILeaseDataFilterBuilder<TFilter> builder)
    {
        return builder.StateIdIn(StateId);
    }

    public TSort GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.StateId(true),
            builder.StateVersion(true)
        );
    }

    public TFilter GetFilter<TFilter>(IMessageDataFilterBuilder<TFilter> builder)
    {
        return builder.StateIdIn(StateId);
    }

    public TSort GetSort<TSort>(IMessageDataSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.StateId(true),
            builder.StateVersion(true)
        );
    }

    public TFilter GetFilter<TFilter>(ISourceDataFilterBuilder<TFilter> builder)
    {
        return builder.AnyStateIdIn(StateId);
    }

    public TSort GetSort<TSort>(ISourceDataSortBuilder<TSort> builder)
    {
        return builder.StateIds(true);
    }

    public int? Skip => null;

    public int? Take => null;

    public TFilter GetFilter<TFilter>(ITagDataFilterBuilder<TFilter> builder)
    {
        return builder.StateIdIn(StateId);
    }

    public TSort GetSort<TSort>(ITagSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.StateId(true),
            builder.StateVersion(true)
        );
    }
}
