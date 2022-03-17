using EntityDb.MongoDb.Sessions;
using System.Threading;
using System.Threading.Tasks;

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
