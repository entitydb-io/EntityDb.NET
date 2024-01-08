using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.FilterBuilders;

internal sealed class TagFilterBuilder : MessageFilterBuilder,
    ITagFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> TagLabelEq(string label)
    {
        return Eq(nameof(TagDocument.Label), label);
    }

    public FilterDefinition<BsonDocument> TagValueEq(string value)
    {
        return Eq(nameof(TagDocument.Value), value);
    }
}
