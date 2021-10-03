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
    }
}
