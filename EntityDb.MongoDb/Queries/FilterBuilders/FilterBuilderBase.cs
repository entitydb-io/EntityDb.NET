using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Common.Extensions;
using EntityDb.Common.Strategies.Resolving;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Queries.FilterDefinitions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.FilterBuilders
{
    internal abstract class FilterBuilderBase : IFilterBuilder<FilterDefinition<BsonDocument>>
    {
        private static readonly FilterDefinitionBuilder<BsonDocument> _filter = Builders<BsonDocument>.Filter;
        private static readonly string _dataTypeNameFieldName = $"{nameof(DocumentBase.Data)}.{nameof(DocumentBase.Data.TypeName)}";
        private static readonly string _dataValueFieldName = $"{nameof(DocumentBase.Data)}.{nameof(DocumentBase.Data.Value)}";

        protected static FilterDefinition<BsonDocument> In<TValue>(string fieldName, TValue[] values)
        {
            return _filter.In(fieldName, values);
        }

        protected static FilterDefinition<BsonDocument> AnyIn<TValue>(string fieldName, TValue[] values)
        {
            return _filter.In(fieldName, values);
        }

        protected static FilterDefinition<BsonDocument> Gte<TValue>(string fieldName, TValue value)
        {
            return _filter.Gte(fieldName, value);
        }

        protected static FilterDefinition<BsonDocument> Lte<TValue>(string fieldName, TValue value)
        {
            return _filter.Lte(fieldName, value);
        }

        protected static FilterDefinition<BsonDocument> DataTypeIn(params Type[] dataTypes)
        {
            var typeNames = dataTypes.GetTypeNames();

            return _filter.In(_dataTypeNameFieldName, typeNames);
        }

        protected virtual string[] GetHoistedFieldNames()
        {
            return Array.Empty<string>();
        }

        protected FilterDefinition<BsonDocument> DataValueMatches<TData>(Expression<Func<TData, bool>> dataExpression)
        {
            var dataFilter = Builders<TData>.Filter.Where(dataExpression);

            return new EmbeddedFilterDefinition<BsonDocument, TData>(_dataValueFieldName, dataFilter, GetHoistedFieldNames());
        }

        public FilterDefinition<BsonDocument> TransactionTimeStampGte(DateTime timeStamp)
        {
            return Gte(nameof(DocumentBase.TransactionTimeStamp), timeStamp);
        }

        public FilterDefinition<BsonDocument> TransactionTimeStampLte(DateTime timeStamp)
        {
            return Lte(nameof(DocumentBase.TransactionTimeStamp), timeStamp);
        }

        public FilterDefinition<BsonDocument> TransactionIdIn(params Guid[] transactionIds)
        {
            return In(nameof(DocumentBase.TransactionId), transactionIds);
        }

        public FilterDefinition<BsonDocument> Not(FilterDefinition<BsonDocument> filter)
        {
            return _filter.Not(filter);
        }

        public FilterDefinition<BsonDocument> And(params FilterDefinition<BsonDocument>[] filters)
        {
            return _filter.And(filters);
        }

        public FilterDefinition<BsonDocument> Or(params FilterDefinition<BsonDocument>[] filters)
        {
            return _filter.Or(filters);
        }
    }
}
