using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Sources.Documents;
using EntityDb.MongoDb.Sources.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace EntityDb.MongoDb.Documents.Queries;

internal static class DocumentQueryExtensions
{
    private static IAsyncEnumerable<Id> EnumerateIds<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        ProjectionDefinition<BsonDocument, TDocument> projection,
        Func<IAsyncEnumerable<TDocument>, IAsyncEnumerable<Id>> mapToIds,
        CancellationToken cancellationToken
    )
    {
        var skip = documentQuery.Skip;
        var limit = documentQuery.Limit;

        documentQuery = documentQuery with { Skip = null, Limit = null };

        var documents = documentQuery.Execute(mongoSession, projection, cancellationToken);

        return documents.EnumerateIds(skip, limit, mapToIds);
    }

    private static IAsyncEnumerable<Pointer> EnumeratePointers<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        ProjectionDefinition<BsonDocument, TDocument> projection,
        Func<IAsyncEnumerable<TDocument>, IAsyncEnumerable<Pointer>> mapToPointers,
        CancellationToken cancellationToken
    )
    {
        var skip = documentQuery.Skip;
        var limit = documentQuery.Limit;

        documentQuery = documentQuery with { Skip = null, Limit = null };

        var documents = documentQuery.Execute(mongoSession, projection, cancellationToken);

        return documents.EnumeratePointers(skip, limit, mapToPointers);
    }

    public static IAsyncEnumerable<Id> EnumerateSourceIds<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        CancellationToken cancellationToken
    )
        where TDocument : DocumentBase
    {
        return documentQuery.EnumerateIds
        (
            mongoSession,
            DocumentBase.SourceIdProjection,
            documents => documents.Select(document => document.SourceId),
            cancellationToken
        );
    }

    public static IAsyncEnumerable<Pointer> EnumerateMessageGroupEntityPointers<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        CancellationToken cancellationToken
    )
        where TDocument : MessageGroupDocumentBase
    {
        return documentQuery.EnumeratePointers
        (
            mongoSession,
            MessageGroupDocumentBase.EntityPointersProjection,
            documents => documents.SelectMany(document => AsyncEnumerablePolyfill.FromResult(document.EntityPointers)),
            cancellationToken
        );
    }

    public static IAsyncEnumerable<Pointer> EnumerateMessageEntityPointers<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        CancellationToken cancellationToken
    )
        where TDocument : MessageDocumentBase
    {
        return documentQuery.EnumeratePointers
        (
            mongoSession,
            MessageDocumentBase.EntityPointerProjection,
            documents => documents.Select(document => document.EntityPointer),
            cancellationToken
        );
    }

    public static async IAsyncEnumerable<TData> EnumerateData<TDocument, TData>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
        where TDocument : DocumentBase
    {
        var documents = documentQuery.Execute(mongoSession, DocumentBase.DataProjection, cancellationToken);

        await foreach (var document in documents)
        {
            yield return envelopeService.Deserialize<TData>(document.Data);
        }
    }

    public static IAsyncEnumerable<IAnnotatedSourceData<TData>> EnumerateEntityAnnotation<TDocument, TData>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService,
        CancellationToken cancellationToken
    )
        where TDocument : MessageDocumentBase
        where TData : notnull
    {
        var documents = documentQuery.Execute(mongoSession, DocumentBase.NoIdProjection, cancellationToken);

        return documents.EnumerateEntityAnnotation<TDocument, BsonDocument, TData>(envelopeService, cancellationToken);
    }

    public static IAsyncEnumerable<IAnnotatedSourceGroupData<TData>> EnumerateEntitiesAnnotation<TDocument, TData>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService,
        CancellationToken cancellationToken
    )
        where TDocument : MessageGroupDocumentBase
        where TData : notnull
    {
        var documents = documentQuery.Execute(mongoSession, DocumentBase.NoIdProjection, cancellationToken);

        return documents.EnumerateEntitiesAnnotation<TDocument, BsonDocument, TData>(envelopeService,
            cancellationToken);
    }
}
