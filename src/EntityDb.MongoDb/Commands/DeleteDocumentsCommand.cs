using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands;

internal record DeleteDocumentsCommand
(
    string CollectionName,
    FilterDefinition<BsonDocument> FilterDefinition
)
{
    public async Task Execute(IMongoSession mongoSession)
    {
        await mongoSession
            .Delete(CollectionName, FilterDefinition);
    }
}
