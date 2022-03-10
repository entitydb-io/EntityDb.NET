using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
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

    public FilterDefinition<BsonDocument> LeaseMatches<TLease>(Expression<Func<TLease, bool>> leaseExpression)
    {
        return DataValueMatches(leaseExpression);
    }

    protected override string[] GetHoistedFieldNames()
    {
        return LeaseDocument.HoistedFieldNames;
    }
}
