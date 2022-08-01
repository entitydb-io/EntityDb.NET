using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.SqlDb.Documents.AgentSignature;
using EntityDb.SqlDb.Queries.Definitions.Sort;

namespace EntityDb.SqlDb.Queries.SortBuilders;

internal sealed class AgentSignatureSortBuilder : SortBuilderBase, IAgentSignatureSortBuilder<ISortDefinition>
{
    public ISortDefinition EntityIds(bool ascending)
    {
        return Sort(ascending, nameof(AgentSignatureDocument.EntityIds));
    }

    public ISortDefinition AgentSignatureType(bool ascending)
    {
        return Sort(ascending, nameof(AgentSignatureDocument.DataType));
    }
}
