using EntityDb.MongoDb.Sessions;

namespace EntityDb.MongoDb.Commands;

internal record InsertDocumentsCommand<TDocument>
(
    string CollectionName,
    TDocument[] Documents
)
{
    public async Task Execute(IMongoSession mongoSession, CancellationToken cancellationToken)
    {
        await mongoSession
            .Insert(CollectionName, Documents, cancellationToken);
    }
}
