using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.FilterBuilders;

internal sealed class AgentSignatureFilterBuilder : FilterBuilderBase,
    IAgentSignatureFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> EntityIdsIn(params Id[] entityIds)
    {
        return AnyIn(nameof(AgentSignatureDocument.EntityIds), entityIds);
    }

    public FilterDefinition<BsonDocument> AgentSignatureTypeIn(params Type[] agentSignatureTypes)
    {
        return DataTypeIn(agentSignatureTypes);
    }
}
