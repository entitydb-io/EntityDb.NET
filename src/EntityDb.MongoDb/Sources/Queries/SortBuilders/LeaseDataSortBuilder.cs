using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.SortBuilders;

internal sealed class LeaseDataSortBuilder : MessageSortBuilder, ILeaseDataSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> LeaseScope(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDataDocument.Scope));
    }

    public SortDefinition<BsonDocument> LeaseLabel(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDataDocument.Label));
    }

    public SortDefinition<BsonDocument> LeaseValue(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDataDocument.Value));
    }
}
