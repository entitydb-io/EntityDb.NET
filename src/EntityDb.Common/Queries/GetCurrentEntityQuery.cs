using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Queries;

internal sealed record GetCurrentEntityQuery(Id EntityId, VersionNumber StartAfterVersionNumber) : GetEntityQuery(EntityId)
{
    protected override TFilter GetSubFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
    {
        return builder.EntityVersionNumberGte(StartAfterVersionNumber.Next());
    }
}
