using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.FilterBuilders
{
    internal sealed class SourceFilterBuilder : FilterBuilderBase, ISourceFilterBuilder<FilterDefinition<BsonDocument>>
    {
        public FilterDefinition<BsonDocument> EntityIdsIn(params Guid[] entityIds)
        {
            return AnyIn(nameof(SourceDocument.EntityIds), entityIds);
        }

        public FilterDefinition<BsonDocument> SourceTypeIn(params Type[] sourceTypes)
        {
            return DataTypeIn(sourceTypes);
        }

        public FilterDefinition<BsonDocument> SourceMatches<TSource>(Expression<Func<TSource, bool>> sourceExpression)
        {
            return DataValueMatches(sourceExpression);
        }
    }
}
