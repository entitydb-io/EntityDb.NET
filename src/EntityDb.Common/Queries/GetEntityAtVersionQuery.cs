using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Queries;

internal sealed record GetEntityAtVersionQuery(Id EntityId, VersionNumber LteVersionNumber) : GetEntityQuery(EntityId)
{
    protected override TFilter GetSubFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
    {
        return builder.EntityVersionNumberLte(LteVersionNumber);
    }
}
