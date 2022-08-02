using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.SqlDb.Documents.AgentSignature;
using EntityDb.SqlDb.Queries.Definitions.Filter;

namespace EntityDb.SqlDb.Queries.FilterBuilders;

internal sealed class AgentSignatureFilterBuilder : FilterBuilderBase, IAgentSignatureFilterBuilder<IFilterDefinition>
{
    public IFilterDefinition EntityIdsIn(params Id[] entityIds)
    {
        return AnyIn(nameof(AgentSignatureDocument.EntityIds), entityIds);
    }

    public IFilterDefinition AgentSignatureTypeIn(params Type[] agentSignatureTypes)
    {
        return DataTypeIn(agentSignatureTypes);
    }
}
