using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.Filters;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.AgentSignature;
using EntityDb.Common.Tests.Implementations.Tags;

namespace EntityDb.Common.Tests.Implementations.Queries
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
