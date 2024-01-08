using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.FilterBuilders;

internal sealed class LeaseFilterBuilder : MessageFilterBuilder,
    ILeaseFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> LeaseScopeEq(string scope)
    {
        return Eq(nameof(LeaseDocument.Scope), scope);
    }

    public FilterDefinition<BsonDocument> LeaseLabelEq(string label)
    {
        return Eq(nameof(LeaseDocument.Label), label);
    }

    public FilterDefinition<BsonDocument> LeaseValueEq(string value)
    {
        return Eq(nameof(LeaseDocument.Value), value);
    }
}
