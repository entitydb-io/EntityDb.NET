using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.SortBuilders;

internal sealed class LeaseSortBuilder : SortBuilderBase, ILeaseSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> EntityId(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.EntityId));
    }

    public SortDefinition<BsonDocument> EntityVersionNumber(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.EntityVersionNumber));
    }

    public SortDefinition<BsonDocument> LeaseType(bool ascending)
    {
        return SortDataType(ascending);
    }

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
