using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Annotations;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Polyfills;
using EntityDb.SqlDb.Documents;
using EntityDb.SqlDb.Queries;
using EntityDb.SqlDb.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EntityDb.SqlDb.Extensions;

internal static class DocumentQueryExtensions
{

    private static IAsyncEnumerable<Id> EnumerateIds<TDocument, TOptions>
    (
        this DocumentQuery<TDocument> documentQuery,
        ISqlDbSession<TOptions> sqlDbSession,
        IDocumentReader<TDocument> documentReader,
        Func<IAsyncEnumerable<TDocument>, IAsyncEnumerable<Id>> mapToIds,
        CancellationToken cancellationToken
    )
        where TDocument : ITransactionDocument
        where TOptions : class
    {
        var skip = documentQuery.Skip;
        var limit = documentQuery.Limit;

        documentQuery = documentQuery with { Skip = null, Limit = null };

        var documents = documentQuery.Execute(sqlDbSession, documentReader, cancellationToken);

        var ids = mapToIds
            .Invoke(documents)
            .Distinct();

        if (skip.HasValue)
        {
            ids = ids.Skip(skip.Value);
        }

        if (limit.HasValue)
        {
            ids = ids.Take(limit.Value);
        }

        return ids;
    }

    public static IAsyncEnumerable<Id> EnumerateTransactionIds<TDocument, TOptions>
    (
        this DocumentQuery<TDocument> documentQuery,
        ISqlDbSession<TOptions> sqlDbSession,
        CancellationToken cancellationToken
    )
        where TOptions : class
        where TDocument : ITransactionDocument<TDocument>
    {
        return documentQuery.EnumerateIds
        (
            sqlDbSession,
            TDocument.TransactionIdDocumentReader,
            documents => documents.Select(document => document.TransactionId),
            cancellationToken
        );
    }

    public static IAsyncEnumerable<Id> EnumerateEntityIds<TDocument, TOptions>
    (
        this DocumentQuery<TDocument> documentQuery,
        ISqlDbSession<TOptions> sqlDbSession,
        CancellationToken cancellationToken
    )
        where TOptions : class
        where TDocument : IEntityDocument<TDocument>
    {
        return documentQuery.EnumerateIds
        (
            sqlDbSession,
            TDocument.EntityIdDocumentReader,
            documents => documents.Select(document => document.EntityId),
            cancellationToken
        );
    }

    public static IAsyncEnumerable<Id> EnumerateEntitiesIds<TDocument, TOptions>
    (
        this DocumentQuery<TDocument> documentQuery,
        ISqlDbSession<TOptions> sqlDbSession,
        CancellationToken cancellationToken
    )
        where TOptions : class
        where TDocument : IEntitiesDocument<TDocument>
    {
        return documentQuery.EnumerateIds
        (
            sqlDbSession,
            TDocument.EntityIdsDocumentReader,
            documents => documents.SelectMany(document => AsyncEnumerablePolyfill.FromResult(document.EntityIds)),
            cancellationToken
        );
    }

    public static async IAsyncEnumerable<TData> EnumerateData<TDocument, TData, TOptions>
    (
        this DocumentQuery<TDocument> documentQuery,
        ISqlDbSession<TOptions> sqlDbSession,
        IEnvelopeService<string> envelopeService,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
        where TOptions : class
        where TDocument : ITransactionDocument<TDocument>
    {
        var documents = documentQuery.Execute
        (
            sqlDbSession,
            TDocument.DataDocumentReader,
            cancellationToken
        );

        await foreach (var document in documents)
        {
            yield return envelopeService.Deserialize<TData>(document.Data);
        }
    }

    public static async IAsyncEnumerable<IEntityAnnotation<TData>> EnumerateEntityAnnotation<TDocument, TData, TOptions>
    (
        this DocumentQuery<TDocument> documentQuery,
        ISqlDbSession<TOptions> sqlDbSession,
        IEnvelopeService<string> envelopeService,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
        where TOptions : class
        where TDocument : IEntityDocument<TDocument>
        where TData : notnull
    {
        var documents = documentQuery.Execute
        (
            sqlDbSession,
            TDocument.DocumentReader,
            cancellationToken
        );

        await foreach (var document in documents)
        {
            yield return EntityAnnotation<TData>.CreateFromBoxedData
            (
                document.TransactionId,
                document.TransactionTimeStamp,
                document.EntityId,
                document.EntityVersionNumber,
                envelopeService.Deserialize<TData>(document.Data)
            );
        }
    }

    public static async IAsyncEnumerable<IEntitiesAnnotation<TData>> EnumerateEntitiesAnnotation<TDocument, TData, TOptions>
    (
        this DocumentQuery<TDocument> documentQuery,
        ISqlDbSession<TOptions> sqlDbSession,
        IEnvelopeService<string> envelopeService,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
        where TOptions : class
        where TDocument : IEntitiesDocument<TDocument>
        where TData : notnull
    {
        var documents = documentQuery.Execute
        (
            sqlDbSession,
            TDocument.DocumentReader,
            cancellationToken
        );

        await foreach (var document in documents)
        {
            yield return EntitiesAnnotation<TData>.CreateFromBoxedData
            (
                document.TransactionId,
                document.TransactionTimeStamp,
                document.EntityIds,
                envelopeService.Deserialize<TData>(document.Data)
            );
        }
    }
}
