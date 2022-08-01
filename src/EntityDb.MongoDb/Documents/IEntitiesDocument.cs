using EntityDb.Common.Documents;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Documents;

internal interface IEntitiesDocument : IEntitiesDocument<BsonDocument>, ITransactionDocument
{
}
