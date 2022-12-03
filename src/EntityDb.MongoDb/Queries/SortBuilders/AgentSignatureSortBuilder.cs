using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.SortBuilders;

internal sealed class AgentSignatureSortBuilder : SortBuilderBase,
    IAgentSignatureSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> EntityIds(bool ascending)
    {
        return Sort(ascending, nameof(AgentSignatureDocument.EntityIds));
    }

    public SortDefinition<BsonDocument> AgentSignatureType(bool ascending)
    {
        return SortDataType(ascending);
    }
}
