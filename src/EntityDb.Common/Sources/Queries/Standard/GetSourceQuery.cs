using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record GetSourceQuery(Id SourceId) : IMessageGroupQuery, IMessageQuery
{
    public TFilter GetFilter<TFilter>(IMessageGroupFilterBuilder<TFilter> builder)
    {
        return builder.SourceIdIn(SourceId);
    }

    public TSort? GetSort<TSort>(IMessageGroupSortBuilder<TSort> builder)
    {
        return default;
    }

    public int? Skip => default;

    public int? Take => default;

    public object? Options => default;

    public TFilter GetFilter<TFilter>(IMessageFilterBuilder<TFilter> builder)
    {
        return builder.SourceIdIn(SourceId);
    }

    public TSort? GetSort<TSort>(IMessageSortBuilder<TSort> builder)
    {
        return default;
    }
}
