using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands;

internal record DeleteDocumentsCommand
(
    IMongoSession MongoSession,
    string CollectionName,
    FilterDefinition<BsonDocument> FilterDefinition
) : DocumentsCommand
{
    public override async Task Execute()
    {
        await MongoSession
            .Delete(CollectionName, FilterDefinition);
    }
}
