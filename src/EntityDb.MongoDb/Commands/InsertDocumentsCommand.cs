using EntityDb.MongoDb.Sessions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands;

internal record InsertDocumentsCommand<TDocument>
(
    string CollectionName,
    TDocument[] Documents
)
{
    public async Task Execute(IMongoSession mongoSession)
    {
        await mongoSession
            .Insert(CollectionName, Documents);
    }
}
