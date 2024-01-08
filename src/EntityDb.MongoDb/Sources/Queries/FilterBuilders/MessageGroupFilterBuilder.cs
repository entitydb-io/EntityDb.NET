using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.FilterBuilders;

internal sealed class MessageGroupFilterBuilder : FilterBuilderBase,
    IMessageGroupFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> AnyEntityIdIn(params Id[] entityIds)
    {
        return AnyIn
        (
            nameof(MessageGroupDocumentBase.EntityIds),
            entityIds
        );
    }
}
