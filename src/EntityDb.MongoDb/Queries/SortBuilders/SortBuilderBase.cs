using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Queries.SortDefinitions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.SortBuilders
{
    internal abstract class SortBuilderBase : BuilderBase, ISortBuilder<SortDefinition<BsonDocument>>
    {
        private static readonly SortDefinitionBuilder<BsonDocument> _sort = Builders<BsonDocument>.Sort;

        public SortDefinition<BsonDocument> TransactionTimeStamp(bool ascending)
        {
            return Sort(ascending, nameof(DocumentBase.TransactionTimeStamp));
        }

        public SortDefinition<BsonDocument> TransactionId(bool ascending)
        {
            return Sort(ascending, nameof(DocumentBase.TransactionId));
        }

        public SortDefinition<BsonDocument> Combine(params SortDefinition<BsonDocument>[] sorts)
        {
            return _sort.Combine(sorts);
        }

        protected static SortDefinition<BsonDocument> Sort(bool ascending, string fieldName)
        {
            return ascending
                ? _sort.Ascending(fieldName)
                : _sort.Descending(fieldName);
        }

        protected static SortDefinition<BsonDocument> SortDataType(bool ascending)
        {
            return Sort(ascending, DataTypeNameFieldName);
        }

        protected virtual string[] GetHoistedFieldNames()
        {
            return Array.Empty<string>();
        }

        protected SortDefinition<BsonDocument> SortDataValue<TData>(bool ascending,
            Expression<Func<TData, object>> dataExpression)
        {
            var dataSort = ascending
                ? Builders<TData>.Sort.Ascending(dataExpression)
                : Builders<TData>.Sort.Descending(dataExpression);

            return new EmbeddedSortDefinition<BsonDocument, TData>(DataValueFieldName, dataSort,
                GetHoistedFieldNames());
        }
    }
}
