using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Queries.Modified
{
    internal sealed record ModifiedAgentSignatureQuery
        (IAgentSignatureQuery AgentSignatureQuery, ModifiedQueryOptions ModifiedQueryOptions) : ModifiedQueryBase(AgentSignatureQuery,
            ModifiedQueryOptions), IAgentSignatureQuery
    {
        public TFilter GetFilter<TFilter>(IAgentSignatureFilterBuilder<TFilter> builder)
        {
            if (ModifiedQueryOptions.InvertFilter)
            {
                return builder.Not
                (
                    AgentSignatureQuery.GetFilter(builder)
                );
            }

            return AgentSignatureQuery.GetFilter(builder);
        }

        public TSort? GetSort<TSort>(IAgentSignatureSortBuilder<TSort> builder)
        {
            if (ModifiedQueryOptions.ReverseSort)
            {
                return AgentSignatureQuery.GetSort(builder.Reverse());
            }

            return AgentSignatureQuery.GetSort(builder);
        }
    }
}
