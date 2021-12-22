using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.Filters;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Leases;
using EntityDb.TestImplementations.AgentSignature;
using EntityDb.TestImplementations.Tags;

namespace EntityDb.TestImplementations.Queries
{
    public record CountFilter : IAgentSignatureFilter, ICommandFilter, ILeaseFilter, ITagFilter
    {
        public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            return builder.CommandTypeIn(typeof(Count));
        }

        public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
        {
            return builder.LeaseTypeIn(typeof(CountLease));
        }

        public TFilter GetFilter<TFilter>(IAgentSignatureFilterBuilder<TFilter> builder)
        {
            return builder.AgentSignatureTypeIn(typeof(CounterAgentSignature));
        }

        public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
        {
            return builder.TagTypeIn(typeof(CountTag));
        }
    }
}
