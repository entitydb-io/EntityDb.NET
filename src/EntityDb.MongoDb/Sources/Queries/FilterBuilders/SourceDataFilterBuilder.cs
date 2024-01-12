using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.FilterBuilders;

internal sealed class SourceDataFilterBuilder : DataFilterBuilderBase,
    ISourceDataFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> AnyStateIdIn(params Id[] stateIds)
    {
        return AnyIn
        (
            nameof(SourceDataDocumentBase.StateIds),
            stateIds
        );
    }
}
