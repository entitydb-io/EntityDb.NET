using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.FilterBuilders;

internal sealed class LeaseFilterBuilder : FilterBuilderBase, ILeaseFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> EntityIdIn(params Id[] entityIds)
    {
        return In(nameof(LeaseDocument.EntityId), entityIds);
    }

    public FilterDefinition<BsonDocument> EntityVersionNumberGte(VersionNumber entityVersionNumber)
    {
        return Gte(nameof(LeaseDocument.EntityVersionNumber), entityVersionNumber);
    }

    public FilterDefinition<BsonDocument> EntityVersionNumberLte(VersionNumber entityVersionNumber)
    {
        return Lte(nameof(LeaseDocument.EntityVersionNumber), entityVersionNumber);
    }

    public FilterDefinition<BsonDocument> LeaseTypeIn(params Type[] leaseTypes)
    {
        return DataTypeIn(leaseTypes);
    }

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

    [Obsolete("This method will be removed in the future, and may not be supported for all implementations.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    public FilterDefinition<BsonDocument> LeaseMatches<TLease>(Expression<Func<TLease, bool>> leaseExpression)
    {
        return DataValueMatches(leaseExpression);
    }

    protected override string[] GetHoistedFieldNames()
    {
        return LeaseDocument.HoistedFieldNames;
    }
}
