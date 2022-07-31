using EntityDb.Abstractions.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EntityDb.MongoDb.Documents;

internal abstract record DocumentBase : ITransactionDocument
{
    [BsonIgnoreIfNull] public ObjectId? _id { get; init; }

    public TimeStamp TransactionTimeStamp { get; init; }
    public Id TransactionId { get; init; }
    public BsonDocument Data { get; init; } = default!;
}
