using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.SortBuilders
{
    internal sealed class FactSortBuilder : SortBuilderBase, IFactSortBuilder<SortDefinition<BsonDocument>>
    {
        public SortDefinition<BsonDocument> EntityId(bool ascending)
        {
            return Sort(ascending, nameof(FactDocument.EntityId));
        }

        public SortDefinition<BsonDocument> EntityVersionNumber(bool ascending)
        {
            return Sort(ascending, nameof(FactDocument.EntityVersionNumber));
        }

        public SortDefinition<BsonDocument> EntitySubversionNumber(bool ascending)
        {
            return Sort(ascending, nameof(FactDocument.EntitySubversionNumber));
        }

        public SortDefinition<BsonDocument> FactType(bool ascending)
        {
            return SortDataType(ascending);
        }

        public SortDefinition<BsonDocument> FactProperty<TFact>(bool ascending, Expression<Func<TFact, object>> factExpression)
        {
            return SortDataValue(ascending, factExpression);
        }
    }
}
