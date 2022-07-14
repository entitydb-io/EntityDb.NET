using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;
using System.Collections.Generic;

namespace EntityDb.Common.Queries;

internal sealed record GetEntityCommandsQuery
    (Pointer EntityPointer, VersionNumber SnapshotVersionNumber, object? Options = null) : ICommandQuery
{
    public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
    {
        var filters = new List<TFilter>
        {
            builder.EntityIdIn(EntityPointer.Id), builder.EntityVersionNumberGte(SnapshotVersionNumber.Next())
        };

        if (EntityPointer.VersionNumber != VersionNumber.MinValue)
        {
            filters.Add(builder.EntityVersionNumberLte(EntityPointer.VersionNumber));
        }

        return builder.And(filters.ToArray());
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
