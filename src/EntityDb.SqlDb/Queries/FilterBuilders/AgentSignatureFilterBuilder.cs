using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.SqlDb.Documents.AgentSignature;
using EntityDb.SqlDb.Queries.Definitions.Filter;

namespace EntityDb.SqlDb.Queries.FilterBuilders;

internal sealed class AgentSignatureFilterBuilder : FilterBuilderBase, IAgentSignatureFilterBuilder<IFilterDefinition>
{
    public IFilterDefinition SubjectIdsIn(params Id[] subjectIds)
    {
        return AnyIn(nameof(AgentSignatureDocument.EntityIds), subjectIds);
    }

    public IFilterDefinition AgentSignatureTypeIn(params Type[] agentSignatureTypes)
    {
        return DataTypeIn(agentSignatureTypes);
    }
}
