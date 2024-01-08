using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record GetDeltasQuery(Pointer EntityPointer, Version SnapshotVersion,
    object? Options = null) : IMessageQuery
{
    public TFilter GetFilter<TFilter>(IMessageFilterBuilder<TFilter> builder)
    {
        var filters = new List<TFilter>
        {
            builder.EntityIdIn(EntityPointer.Id), builder.EntityVersionGte(EntityPointer.Version.Next()),
        };

        if (SnapshotVersion != Version.Zero)
        {
            filters.Add(builder.EntityVersionLte(SnapshotVersion));
        }

        return builder.And(filters.ToArray());
    }

    public TSort GetSort<TSort>(IMessageSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.EntityVersion(true)
        );
    }

    public int? Skip => null;

    public int? Take => null;
}
