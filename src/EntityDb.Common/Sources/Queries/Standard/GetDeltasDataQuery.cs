using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.States;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record GetDeltasDataQuery(StatePointer StatePointer, StateVersion KnownStateVersion,
    object? Options = null) : IMessageDataQuery
{
    public TFilter GetFilter<TFilter>(IMessageDataFilterBuilder<TFilter> builder)
    {
        var filters = new List<TFilter>
        {
            builder.StateIdIn(StatePointer.Id), builder.StateVersionGte(KnownStateVersion.Next()),
        };

        if (StatePointer.StateVersion != StateVersion.Zero)
        {
            filters.Add(builder.StateVersionLte(StatePointer.StateVersion));
        }

        return builder.And(filters.ToArray());
    }

    public TSort GetSort<TSort>(IMessageDataSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.StateVersion(true)
        );
    }

    public int? Skip => null;

    public int? Take => null;
}
