﻿using EntityDb.Abstractions.Disposables;
using EntityDb.SqlDb.Documents;
using EntityDb.SqlDb.Queries.Definitions.Filter;
using EntityDb.SqlDb.Queries.Definitions.Sort;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Sessions;

internal interface ISqlDbSession<TOptions> : IDisposableResource
    where TOptions : class
{
    DbConnection DbConnection { get; }

    Task Insert<TDocument>
    (

        string tableName,
        TDocument[] documents,
        CancellationToken cancellationToken
    ) where TDocument : ITransactionDocument;

    IAsyncEnumerable<TDocument> Find<TDocument>
    (
        string tableName,
        IDocumentReader<TDocument> documentReader,
        IFilterDefinition filterDefinition,
        ISortDefinition? sortDefinition,
        int? skip,
        int? limit,
        TOptions? options,
        CancellationToken cancellationToken
    );

    Task Delete
    (
        string tableName,
        IFilterDefinition filterDefinition,
        CancellationToken cancellationToken
    );

    Task StartTransaction(CancellationToken cancellationToken = default);
    Task CommitTransaction(CancellationToken cancellationToken = default);
    Task AbortTransaction(CancellationToken cancellationToken = default);

    ISqlDbSession<TOptions> WithTransactionSessionOptions(SqlDbTransactionSessionOptions transactionSessionOptions);
}