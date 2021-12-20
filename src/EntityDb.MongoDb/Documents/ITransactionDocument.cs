using EntityDb.MongoDb.Envelopes;
using MongoDB.Bson;
using System;

namespace EntityDb.MongoDb.Documents
{
    internal interface ITransactionDocument
    {
#pragma warning disable IDE1006 // Naming Styles
        ObjectId? _id { get; }
#pragma warning restore IDE1006 // Naming Styles

        Guid TransactionId { get; }

        DateTime TransactionTimeStamp { get; }

        BsonDocumentEnvelope Data { get; }
    }
}
