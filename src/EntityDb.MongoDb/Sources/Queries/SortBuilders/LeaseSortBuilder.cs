using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.SortBuilders;

internal sealed class LeaseSortBuilder : MessageSortBuilder, ILeaseSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> LeaseScope(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.Scope));
    }

    public SortDefinition<BsonDocument> LeaseLabel(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.Label));
    }

    public SortDefinition<BsonDocument> LeaseValue(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.Value));
    }
}
