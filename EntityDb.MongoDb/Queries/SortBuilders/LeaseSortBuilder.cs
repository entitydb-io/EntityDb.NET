using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.SortBuilders
{
    internal sealed class LeaseSortBuilder : SortBuilderBase, ILeaseSortBuilder<SortDefinition<BsonDocument>>
    {
        protected override string[] GetHoistedFieldNames()
        {
            return LeaseDocument.HoistedFieldNames;
        }

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

        public SortDefinition<BsonDocument> LeaseProperty<TLease>(bool ascending, System.Linq.Expressions.Expression<System.Func<TLease, object>> leaseExpression)
        {
            return SortDataValue(ascending, leaseExpression);
        }
    }
}
