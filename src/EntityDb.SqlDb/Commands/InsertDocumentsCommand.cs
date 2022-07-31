using EntityDb.SqlDb.Documents;
using EntityDb.SqlDb.Sessions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Commands;

internal record InsertDocumentsCommand<TDocument>
(
    string TableName,
    TDocument[] Documents
)
    where TDocument : ITransactionDocument
{
    public async Task Execute<TOptions>(ISqlDbSession<TOptions> sqlDbSession, CancellationToken cancellationToken)
        where TOptions : class
    {
        await sqlDbSession
            .Insert(TableName, Documents, cancellationToken);
    }
}
