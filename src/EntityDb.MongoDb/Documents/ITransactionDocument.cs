using EntityDb.Abstractions.ValueObjects;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Documents;

internal interface ITransactionDocument
{
#pragma warning disable IDE1006 // Naming Styles
    // ReSharper disable once InconsistentNaming
    ObjectId? _id { get; }
#pragma warning restore IDE1006 // Naming Styles

    Id TransactionId { get; }

    TimeStamp TransactionTimeStamp { get; }

    BsonDocument Data { get; }
}
