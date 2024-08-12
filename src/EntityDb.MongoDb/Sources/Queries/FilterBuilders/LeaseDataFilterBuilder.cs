using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.FilterBuilders;

internal sealed class LeaseDataFilterBuilder : MessageDataFilterBuilder,
    ILeaseDataFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> LeaseScopeEq(string scope)
    {
        return Eq(nameof(LeaseDataDocument.Scope), scope);
    }

    public FilterDefinition<BsonDocument> LeaseLabelEq(string label)
    {
        return Eq(nameof(LeaseDataDocument.Label), label);
    }

    public FilterDefinition<BsonDocument> LeaseValueEq(string value)
    {
        return Eq(nameof(LeaseDataDocument.Value), value);
    }
}
