using EntityDb.SqlDb.Queries.Definitions.Filter;
using EntityDb.SqlDb.Sessions;

namespace EntityDb.SqlDb.Commands;

internal record DeleteDocumentsCommand
(
    string TableName,
    IFilterDefinition FilterDefinition
)
{
    public async Task Execute<TOptions>(ISqlDbSession<TOptions> sqlDbSession, CancellationToken cancellationToken)
        where TOptions : class
    {
        await sqlDbSession
            .Delete(TableName, FilterDefinition, cancellationToken);
    }
}
