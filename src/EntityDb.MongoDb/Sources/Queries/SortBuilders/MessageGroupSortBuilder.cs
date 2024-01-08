using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.SortBuilders;

internal sealed class MessageGroupSortBuilder : SortBuilderBase,
    IMessageGroupSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> EntityIds(bool ascending)
    {
        return Sort(ascending, nameof(AgentSignatureDocument.EntityIds));
    }
}
