using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.SortBuilders;

internal class MessageDataSortBuilder : DataSortBuilderBase,
    IMessageDataSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> StateId(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDataDocument.StateId));
    }

    public SortDefinition<BsonDocument> StateVersion(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDataDocument.StateVersion));
    }
}
