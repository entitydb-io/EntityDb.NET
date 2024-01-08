using EntityDb.MongoDb.Sources.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Documents.Commands;

internal record DeleteDocumentsCommand
(
    string CollectionName,
    FilterDefinition<BsonDocument> FilterDefinition
)
{
    public async Task Execute(IMongoSession mongoSession, CancellationToken cancellationToken)
    {
        await mongoSession
            .Delete(CollectionName, FilterDefinition, cancellationToken);
    }
}
