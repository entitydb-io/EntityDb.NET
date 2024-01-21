using EntityDb.MongoDb.Sources.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Documents.Commands;

internal sealed record DeleteDocumentsCommand
{
    public required string CollectionName { get; init; }
    public required FilterDefinition<BsonDocument> FilterDefinition { get; init; }

    public async Task Execute(IMongoSession mongoSession, CancellationToken cancellationToken)
    {
        await mongoSession
            .Delete(CollectionName, FilterDefinition, cancellationToken);
    }
}
