using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries.FilterBuilders;

internal abstract class DataFilterBuilderBase : IDataFilterBuilder<FilterDefinition<BsonDocument>>
{
    private static readonly FilterDefinitionBuilder<BsonDocument> FilterBuilder = Builders<BsonDocument>.Filter;

    public FilterDefinition<BsonDocument> SourceTimeStampGte(TimeStamp timeStamp)
    {
        return Gte(nameof(DocumentBase.SourceTimeStamp), timeStamp);
    }

    public FilterDefinition<BsonDocument> SourceTimeStampLte(TimeStamp timeStamp)
    {
        return Lte(nameof(DocumentBase.SourceTimeStamp), timeStamp);
    }

    public FilterDefinition<BsonDocument> SourceIdIn(params Id[] sourceIds)
    {
        return In(nameof(DocumentBase.SourceId), sourceIds);
    }

    public FilterDefinition<BsonDocument> DataTypeIn(params Type[] dataTypes)
    {
        var typeNames = dataTypes.GetTypeHeaderValues();

        return FilterBuilder.In(nameof(DocumentBase.DataType), typeNames);
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

    protected static FilterDefinition<BsonDocument> AnyIn<TValue>(string fieldName,
        IEnumerable<TValue> values)
    {
        return FilterBuilder.AnyIn(fieldName, values);
    }

    protected static FilterDefinition<BsonDocument> Gte<TValue>(string fieldName, TValue value)
    {
        return FilterBuilder.Gte(fieldName, value);
    }

    protected static FilterDefinition<BsonDocument> Lte<TValue>(string fieldName, TValue value)
    {
        return FilterBuilder.Lte(fieldName, value);
    }
}
