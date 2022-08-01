using EntityDb.SqlDb.Documents;
using EntityDb.SqlDb.Queries.Definitions.Filter;
using EntityDb.SqlDb.Queries.Definitions.Sort;
using EntityDb.SqlDb.Sessions;

namespace EntityDb.SqlDb.Queries;

internal record DocumentQuery<TDocument>
(
    IFilterDefinition FilterDefinition,
    ISortDefinition? SortDefinition,
    int? Skip,
    int? Limit,
    object? Options
)
    where TDocument : ITransactionDocument
{
    public IAsyncEnumerable<TDocument> Execute<TOptions>(ISqlDbSession<TOptions> sqlDbSession, IDocumentReader<TDocument> documentReader, CancellationToken cancellationToken)
        where TOptions : class
    {
        return sqlDbSession.Find(documentReader, FilterDefinition, SortDefinition, Skip, Limit, Options as TOptions, cancellationToken);
    }
}
