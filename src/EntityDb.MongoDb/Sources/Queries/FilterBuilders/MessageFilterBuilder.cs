using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.MongoDb.Sources.Queries.FilterBuilders;

internal class MessageFilterBuilder : FilterBuilderBase,
    IMessageFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> EntityIdIn(params Id[] entityIds)
    {
        return Or
        (
            entityIds
                .Select(entityId => Eq(nameof(MessageDocumentBase.EntityId), entityId))
                .ToArray()
        );
    }

    public FilterDefinition<BsonDocument> EntityVersionGte(Version entityVersion)
    {
        return Gte(nameof(MessageDocumentBase.EntityVersion), entityVersion);
    }

    public FilterDefinition<BsonDocument> EntityVersionLte(Version entityVersion)
    {
        return Lte(nameof(MessageDocumentBase.EntityVersion), entityVersion);
    }
}
