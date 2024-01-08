using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.SortBuilders;

internal abstract class SortBuilderBase : ISortBuilder<SortDefinition<BsonDocument>>
{
    private static readonly SortDefinitionBuilder<BsonDocument> SortBuilder = Builders<BsonDocument>.Sort;

    public SortDefinition<BsonDocument> SourceTimeStamp(bool ascending)
    {
        return Sort(ascending, nameof(DocumentBase.SourceTimeStamp));
    }

    public SortDefinition<BsonDocument> SourceId(bool ascending)
    {
        return Sort(ascending, nameof(DocumentBase.SourceId));
    }

    public SortDefinition<BsonDocument> DataType(bool ascending)
    {
        return Sort(ascending, nameof(DocumentBase.DataType));
    }

    public SortDefinition<BsonDocument> Combine(params SortDefinition<BsonDocument>[] sorts)
    {
        return SortBuilder.Combine(sorts);
    }

    protected static SortDefinition<BsonDocument> Sort(bool ascending, string fieldName)
    {
        return ascending
            ? SortBuilder.Ascending(fieldName)
            : SortBuilder.Descending(fieldName);
    }
}
