using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record GetLastStateVersionDataQuery(Id StateId, object? Options = null) : IMessageDataQuery
{
    public TFilter GetFilter<TFilter>(IMessageDataFilterBuilder<TFilter> builder)
    {
        return builder.StateIdIn(StateId);
    }

    public TSort GetSort<TSort>(IMessageDataSortBuilder<TSort> builder)
    {
        return builder.StateVersion(false);
    }

    public int? Skip => null;

    public int? Take => 1;
}
