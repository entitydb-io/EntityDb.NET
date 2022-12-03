using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.FilterBuilders;

internal sealed class TagFilterBuilder : FilterBuilderBase, ITagFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> EntityIdIn(params Id[] entityIds)
    {
        return In(nameof(TagDocument.EntityId), entityIds);
    }

    public FilterDefinition<BsonDocument> EntityVersionNumberGte(VersionNumber entityVersionNumber)
    {
        return Gte(nameof(TagDocument.EntityVersionNumber), entityVersionNumber);
    }

    public FilterDefinition<BsonDocument> EntityVersionNumberLte(VersionNumber entityVersionNumber)
    {
        return Lte(nameof(TagDocument.EntityVersionNumber), entityVersionNumber);
    }

    public FilterDefinition<BsonDocument> TagTypeIn(params Type[] tagTypes)
    {
        return DataTypeIn(tagTypes);
    }

    public FilterDefinition<BsonDocument> TagLabelEq(string label)
    {
        return Eq(nameof(TagDocument.Label), label);
    }

    public FilterDefinition<BsonDocument> TagValueEq(string value)
    {
        return Eq(nameof(TagDocument.Value), value);
    }
}
