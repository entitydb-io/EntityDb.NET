using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Queries.FilterDefinitions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.FilterBuilders;

internal abstract class FilterBuilderBase : BuilderBase, IFilterBuilder<FilterDefinition<BsonDocument>>
{
    private static readonly FilterDefinitionBuilder<BsonDocument> FilterBuilder = Builders<BsonDocument>.Filter;

    public FilterDefinition<BsonDocument> TransactionTimeStampGte(TimeStamp timeStamp)
    {
        return Gte(nameof(DocumentBase.TransactionTimeStamp), timeStamp);
    }

    public FilterDefinition<BsonDocument> TransactionTimeStampLte(TimeStamp timeStamp)
    {
        return Lte(nameof(DocumentBase.TransactionTimeStamp), timeStamp);
    }

    public FilterDefinition<BsonDocument> TransactionIdIn(params Id[] transactionIds)
    {
        return In(nameof(DocumentBase.TransactionId), transactionIds);
    }

    public FilterDefinition<BsonDocument> Not(FilterDefinition<BsonDocument> filter)
    {
        return FilterBuilder.Not(filter);
    }

    public FilterDefinition<BsonDocument> And(params FilterDefinition<BsonDocument>[] filters)
    {
        return FilterBuilder.And(filters);
    }

    public FilterDefinition<BsonDocument> Or(params FilterDefinition<BsonDocument>[] filters)
    {
        return FilterBuilder.Or(filters);
    }

    protected static FilterDefinition<BsonDocument> Eq<TValue>(string fieldName, TValue value)
    {
        return FilterBuilder.Eq(fieldName, value);
    }

    protected static FilterDefinition<BsonDocument> In<TValue>(string fieldName, IEnumerable<TValue> values)
    {
        return FilterBuilder.In(fieldName, values);
    }

    protected static FilterDefinition<BsonDocument> AnyIn<TValue>(string fieldName, IEnumerable<TValue> values)
    {
        return FilterBuilder.In(fieldName, values);
    }

    protected static FilterDefinition<BsonDocument> Gte<TValue>(string fieldName, TValue value)
    {
        return FilterBuilder.Gte(fieldName, value);
    }

    protected static FilterDefinition<BsonDocument> Lte<TValue>(string fieldName, TValue value)
    {
        return FilterBuilder.Lte(fieldName, value);
    }

    protected static FilterDefinition<BsonDocument> DataTypeIn(params Type[] dataTypes)
    {
        var typeNames = dataTypes.GetTypeHeaderValues();

        return FilterBuilder.In(DataTypeNameFieldName, typeNames);
    }

    protected virtual string[] GetHoistedFieldNames()
    {
        return Array.Empty<string>();
    }

    [Obsolete("This method will be removed in the future, and may not be supported for all implementations.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    protected FilterDefinition<BsonDocument> DataValueMatches<TData>(Expression<Func<TData, bool>> dataExpression)
    {
        var dataFilter = Builders<TData>.Filter.Where(dataExpression);

        return new EmbeddedFilterDefinition<BsonDocument, TData>(DataValueFieldName, dataFilter,
            GetHoistedFieldNames());
    }
}
