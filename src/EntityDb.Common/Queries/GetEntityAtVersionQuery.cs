using EntityDb.Abstractions.Queries.FilterBuilders;
using System;

namespace EntityDb.Common.Queries
{
    internal sealed record GetEntityAtVersionQuery(Guid EntityId, ulong LteVersionNumber) : GetEntityQuery(EntityId)
    {
        protected override TFilter GetSubFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            return builder.EntityVersionNumberLte(LteVersionNumber);
        }
    }
}
