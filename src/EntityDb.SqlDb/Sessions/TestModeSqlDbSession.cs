using EntityDb.Common.Disposables;
using EntityDb.SqlDb.Documents;
using EntityDb.SqlDb.Queries.Definitions.Filter;
using EntityDb.SqlDb.Queries.Definitions.Sort;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Sessions;

internal record TestModeSqlDbSession<TOptions>(ISqlDbSession<TOptions> SqlDbSession) : DisposableResourceBaseRecord, ISqlDbSession<TOptions>
    where TOptions : class
{
    public DbConnection DbConnection => SqlDbSession.DbConnection;

    public Task Insert<TDocument>(string tableName, TDocument[] bsonDocuments, CancellationToken cancellationToken)
        where TDocument : ITransactionDocument
    {
        return SqlDbSession.Insert(tableName, bsonDocuments, cancellationToken);
    }

    public IAsyncEnumerable<TDocument> Find<TDocument>
    (
        IDocumentReader<TDocument> documentReader,
        IFilterDefinition filterDefinition,
        ISortDefinition? sortDefinition,
        int? skip,
        int? limit,
        TOptions? options,
        CancellationToken cancellationToken
    )
        where TDocument : ITransactionDocument
    {
        return SqlDbSession.Find
        (
            documentReader,
            filterDefinition,
            sortDefinition,
            skip,
            limit,
            options,
            cancellationToken
        );
    }

    public Task Delete(string tableName,
        IFilterDefinition filterDefinition, CancellationToken cancellationToken)
    {
        return SqlDbSession.Delete(tableName, filterDefinition, cancellationToken);
    }

    public ISqlDbSession<TOptions> WithTransactionSessionOptions(SqlDbTransactionSessionOptions transactionSessionOptions)
    {
        return this with { SqlDbSession = SqlDbSession.WithTransactionSessionOptions(transactionSessionOptions) };
    }

    public Task StartTransaction(CancellationToken cancellationToken = default)
    {
        // Test Mode Transactions are started in the Test Mode Repository Factory
        return Task.CompletedTask;
    }

    public Task CommitTransaction(CancellationToken cancellationToken = default)
    {
        // Test Mode Transactions are never committed
        return Task.CompletedTask;
    }

    public Task AbortTransaction(CancellationToken cancellationToken = default)
    {
        // Test Mode Transactions are aborted in the Test Mode Repository Factory
        return Task.CompletedTask;
    }
}
