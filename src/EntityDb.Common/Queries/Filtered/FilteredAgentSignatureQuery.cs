using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.Filters;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Common.Queries.Filtered
{
    internal sealed record FilteredAgentSignatureQuery
        (IAgentSignatureQuery AgentSignatureQuery, IAgentSignatureFilter AgentSignatureFilter) : FilteredQueryBase(AgentSignatureQuery), IAgentSignatureQuery
    {
        public TFilter GetFilter<TFilter>(IAgentSignatureFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                AgentSignatureQuery.GetFilter(builder),
                AgentSignatureFilter.GetFilter(builder)
            );
        }

        public TSort? GetSort<TSort>(IAgentSignatureSortBuilder<TSort> builder)
        {
            return AgentSignatureQuery.GetSort(builder);
        }
    }
}
