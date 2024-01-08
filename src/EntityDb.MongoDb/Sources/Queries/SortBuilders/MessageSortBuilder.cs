using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.SortBuilders;

internal class MessageSortBuilder : SortBuilderBase,
    IMessageSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> EntityId(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.EntityId));
    }

    public SortDefinition<BsonDocument> EntityVersion(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.EntityVersion));
    }
}
