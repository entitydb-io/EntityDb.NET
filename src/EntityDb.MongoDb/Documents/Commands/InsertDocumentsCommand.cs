using EntityDb.MongoDb.Sources.Sessions;

namespace EntityDb.MongoDb.Documents.Commands;

internal sealed record InsertDocumentsCommand<TDocument>
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
