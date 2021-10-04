using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.SortBuilders
{
    internal sealed class SourceSortBuilder : SortBuilderBase, ISourceSortBuilder<SortDefinition<BsonDocument>>
    {
        public SortDefinition<BsonDocument> EntityIds(bool ascending)
        {
            return Sort(ascending, nameof(SourceDocument.EntityIds));
        }

        public SortDefinition<BsonDocument> SourceType(bool ascending)
        {
            return SortDataType(ascending);
        }

        public SortDefinition<BsonDocument> SourceProperty<TSource>(bool ascending, Expression<Func<TSource, object>> sourceExpression)
        {
            return SortDataValue(ascending, sourceExpression);
        }
    }
}
