using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record GetDeltasQuery(Pointer StatePointer, Version PersistedStateVersion,
    object? Options = null) : IMessageDataQuery
{
    public TFilter GetFilter<TFilter>(IMessageDataFilterBuilder<TFilter> builder)
    {
        var filters = new List<TFilter>
        {
            builder.StateIdIn(StatePointer.Id), builder.StateVersionGte(StatePointer.Version.Next()),
        };

        if (PersistedStateVersion != Version.Zero)
        {
            filters.Add(builder.StateVersionLte(PersistedStateVersion));
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
