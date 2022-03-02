using EntityDb.MongoDb.Sessions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands;

internal record InsertDocumentsCommand<TDocument>
(
    IMongoSession MongoSession,
    string CollectionName,
    TDocument[] Documents
) : DocumentsCommand
{
    public override async Task Execute()
    {
        await MongoSession
            .Insert(CollectionName, Documents);
    }
}
