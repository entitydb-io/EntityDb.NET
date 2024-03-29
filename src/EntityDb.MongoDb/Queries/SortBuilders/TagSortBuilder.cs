using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.SortBuilders;

internal sealed class TagSortBuilder : SortBuilderBase, ITagSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> EntityId(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.EntityId));
    }

    public SortDefinition<BsonDocument> EntityVersionNumber(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.EntityVersionNumber));
    }

    public SortDefinition<BsonDocument> TagType(bool ascending)
    {
        return SortDataType(ascending);
    }

    public SortDefinition<BsonDocument> TagLabel(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.Label));
    }

    public SortDefinition<BsonDocument> TagValue(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.Value));
    }
}
