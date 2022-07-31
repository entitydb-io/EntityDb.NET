using EntityDb.SqlDb.Documents;
using EntityDb.SqlDb.Queries.Definitions.Filter;
using EntityDb.SqlDb.Queries.Definitions.Sort;
using EntityDb.SqlDb.Sessions;
using System.Collections.Generic;
using System.Threading;

namespace EntityDb.SqlDb.Queries;

internal record DocumentQuery<TDocument>
(
    string TableName,
    IFilterDefinition FilterDefinition,
    ISortDefinition? SortDefinition,
    int? Skip,
    int? Limit,
    object? Options
)
{
    public IAsyncEnumerable<TDocument> Execute<TOptions>(ISqlDbSession<TOptions> sqlDbSession, IDocumentReader<TDocument> documentReader, CancellationToken cancellationToken)
        where TOptions : class
    {
        return sqlDbSession.Find(TableName, documentReader, FilterDefinition, SortDefinition, Skip, Limit, Options as TOptions, cancellationToken);
    }
}
