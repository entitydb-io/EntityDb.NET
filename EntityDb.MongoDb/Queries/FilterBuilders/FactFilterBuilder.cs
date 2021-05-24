using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.FilterBuilders
{
    internal sealed class FactFilterBuilder : FilterBuilderBase, IFactFilterBuilder<FilterDefinition<BsonDocument>>
    {
        public FilterDefinition<BsonDocument> EntityIdIn(params Guid[] entityIds)
        {
            return In(nameof(FactDocument.EntityId), entityIds);
        }

        public FilterDefinition<BsonDocument> EntityVersionNumberGte(ulong entityVersionNumber)
        {
            return Gte(nameof(FactDocument.EntityVersionNumber), entityVersionNumber);
        }

        public FilterDefinition<BsonDocument> EntityVersionNumberLte(ulong entityVersionNumber)
        {
            return Lte(nameof(FactDocument.EntityVersionNumber), entityVersionNumber);
        }

        public FilterDefinition<BsonDocument> FactTypeIn(params Type[] factTypes)
        {
            return DataTypeIn(factTypes);
        }

        public FilterDefinition<BsonDocument> FactMatches<TFact>(Expression<Func<TFact, bool>> factExpression)
        {
            return DataValueMatches(factExpression);
        }
    }
}
