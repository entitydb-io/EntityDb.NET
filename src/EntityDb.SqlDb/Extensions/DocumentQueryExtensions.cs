using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Extensions;
using EntityDb.Common.Polyfills;
using EntityDb.SqlDb.Documents;
using EntityDb.SqlDb.Queries;
using EntityDb.SqlDb.Sessions;
using System.Runtime.CompilerServices;

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

        return documents.EnumerateIds(skip, limit, mapToIds);
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
            documents => documents.Select(document => document.GetSourceId()),
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
            documents => documents.Select(document => document.GetSubjectId()),
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
            documents => documents.SelectMany(document => AsyncEnumerablePolyfill.FromResult(document.GetSubjectIds())),
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

    public static IAsyncEnumerable<ISourceSubjectAnnotation<TData>> EnumerateEntityAnnotation<TDocument, TData, TOptions>
    (
        this DocumentQuery<TDocument> documentQuery,
        ISqlDbSession<TOptions> sqlDbSession,
        IEnvelopeService<string> envelopeService,
        CancellationToken cancellationToken
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

        return documents.EnumerateEntityAnnotation<TDocument, string, TData>(envelopeService, cancellationToken);
    }

    public static IAsyncEnumerable<ISourceSubjectsAnnotation<TData>> EnumerateEntitiesAnnotation<TDocument, TData, TOptions>
    (
        this DocumentQuery<TDocument> documentQuery,
        ISqlDbSession<TOptions> sqlDbSession,
        IEnvelopeService<string> envelopeService,
        CancellationToken cancellationToken
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

        return documents.EnumerateEntitiesAnnotation<TDocument, string, TData>(envelopeService, cancellationToken);
    }
}
