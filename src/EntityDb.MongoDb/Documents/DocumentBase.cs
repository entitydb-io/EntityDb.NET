using EntityDb.Abstractions.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EntityDb.MongoDb.Documents;

internal abstract record DocumentBase
{
    [BsonIgnoreIfNull] public ObjectId? _id { get; init; }

    public TimeStamp TransactionTimeStamp { get; init; }
    public Id TransactionId { get; init; }
    public string DataType { get; init; } = default!;
    public BsonDocument Data { get; init; } = default!;
}
