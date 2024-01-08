using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.SortBuilders;

internal sealed class TagSortBuilder : MessageSortBuilder, ITagSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> TagLabel(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.Label));
    }

    public SortDefinition<BsonDocument> TagValue(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.Value));
    }
}
