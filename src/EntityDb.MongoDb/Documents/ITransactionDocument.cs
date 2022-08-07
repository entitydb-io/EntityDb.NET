using EntityDb.Common.Documents;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Documents;

internal interface ITransactionDocument : ITransactionDocument<BsonDocument>
{
#pragma warning disable IDE1006 // Naming Styles
    // ReSharper disable once InconsistentNaming
    ObjectId? _id { get; }
#pragma warning restore IDE1006 // Naming Styles
}
