using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.MongoDb.Sources.Queries.FilterBuilders;

internal class MessageDataFilterBuilder : DataFilterBuilderBase,
    IMessageDataFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> StateIdIn(params Id[] stateIds)
    {
        return Or
        (
            stateIds
                .Select(stateId => Eq(nameof(MessageDataDocumentBase.StateId), stateId))
                .ToArray()
        );
    }

    public FilterDefinition<BsonDocument> StateVersionGte(Version stateVersion)
    {
        return Gte(nameof(MessageDataDocumentBase.StateVersion), stateVersion);
    }

    public FilterDefinition<BsonDocument> StateVersionLte(Version stateVersion)
    {
        return Lte(nameof(MessageDataDocumentBase.StateVersion), stateVersion);
    }
}
