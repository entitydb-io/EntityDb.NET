using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.FilterBuilders;

internal sealed class TagDataFilterBuilder : MessageDataFilterBuilder,
    ITagDataFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> TagLabelEq(string label)
    {
        return Eq(nameof(TagDataDocument.Label), label);
    }

    public FilterDefinition<BsonDocument> TagValueEq(string value)
    {
        return Eq(nameof(TagDataDocument.Value), value);
    }
}
