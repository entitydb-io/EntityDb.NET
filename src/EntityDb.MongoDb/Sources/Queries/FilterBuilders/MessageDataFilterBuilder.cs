using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.States;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

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

    public FilterDefinition<BsonDocument> StateVersionGte(StateVersion stateVersion)
    {
        return Gte(nameof(MessageDataDocumentBase.StateVersion), stateVersion);
    }

    public FilterDefinition<BsonDocument> StateVersionLte(StateVersion stateVersion)
    {
        return Lte(nameof(MessageDataDocumentBase.StateVersion), stateVersion);
    }
}
