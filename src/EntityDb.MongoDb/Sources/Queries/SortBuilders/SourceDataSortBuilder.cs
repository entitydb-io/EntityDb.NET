using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.SortBuilders;

internal sealed class SourceDataSortBuilder : SortBuilderBase,
    ISourceDataSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> StateIds(bool ascending)
    {
        return Sort(ascending, nameof(AgentSignatureDocument.StateIds));
    }
}
