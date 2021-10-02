using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.FilterBuilders
{
    internal sealed class LeaseFilterBuilder : FilterBuilderBase, ILeaseFilterBuilder<FilterDefinition<BsonDocument>>
    {
        protected override string[] GetHoistedFieldNames()
        {
            return LeaseDocument.HoistedFieldNames;
        }

        public FilterDefinition<BsonDocument> EntityIdIn(params Guid[] entityIds)
        {
            return In(nameof(LeaseDocument.EntityId), entityIds);
        }

        public FilterDefinition<BsonDocument> EntityVersionNumberGte(ulong entityVersionNumber)
        {
            return Gte(nameof(LeaseDocument.EntityVersionNumber), entityVersionNumber);
        }

        public FilterDefinition<BsonDocument> EntityVersionNumberLte(ulong entityVersionNumber)
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
    }
}
