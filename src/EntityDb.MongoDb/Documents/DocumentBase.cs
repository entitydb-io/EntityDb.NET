using EntityDb.MongoDb.Envelopes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using System;

namespace EntityDb.MongoDb.Documents;

internal abstract record DocumentBase : ITransactionDocument
{
    static DocumentBase()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
    }

    public DateTime TransactionTimeStamp { get; init; }

    [BsonIgnoreIfNull] public ObjectId? _id { get; init; }

    public Guid TransactionId { get; init; }
    public BsonDocumentEnvelope Data { get; init; } = default!;
}
