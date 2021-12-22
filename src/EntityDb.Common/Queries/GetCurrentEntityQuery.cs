using EntityDb.Abstractions.Queries.FilterBuilders;
using System;

namespace EntityDb.Common.Queries
{
    internal sealed record GetCurrentEntityQuery(Guid EntityId, ulong StartAfterVersionNumber) : GetEntityQuery(EntityId)
    {
        protected override TFilter GetSubFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            return builder.EntityVersionNumberGte(StartAfterVersionNumber + 1);
        }
    }
}
