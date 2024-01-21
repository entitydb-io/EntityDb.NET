using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.States;
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

    private static IAsyncEnumerable<StatePointer> EnumeratePointers<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        ProjectionDefinition<BsonDocument, TDocument> projection,
        Func<IAsyncEnumerable<TDocument>, IAsyncEnumerable<StatePointer>> mapToPointers,
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

    public static IAsyncEnumerable<StatePointer> EnumerateSourceDataStatePointers<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        CancellationToken cancellationToken
    )
        where TDocument : SourceDataDocumentBase
    {
        return documentQuery.EnumeratePointers
        (
            mongoSession,
            SourceDataDocumentBase.StatePointersProjection,
            documents => documents.SelectMany(document => AsyncEnumerablePolyfill.FromResult(document.StatePointers)),
            cancellationToken
        );
    }

    public static IAsyncEnumerable<StatePointer> EnumerateMessageStatePointers<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        CancellationToken cancellationToken
    )
        where TDocument : MessageDataDocumentBase
    {
        return documentQuery.EnumeratePointers
        (
            mongoSession,
            MessageDataDocumentBase.StatePointerProjection,
            documents => documents.Select(document => document.StatePointer),
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

    public static IAsyncEnumerable<IAnnotatedMessageData<TData>> EnumerateAnnotatedSourceData<TDocument, TData>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService,
        CancellationToken cancellationToken
    )
        where TDocument : MessageDataDocumentBase
        where TData : notnull
    {
        var documents = documentQuery.Execute(mongoSession, DocumentBase.NoIdProjection, cancellationToken);

        return documents.EnumerateAnnotatedSourceData<TDocument, BsonDocument, TData>(envelopeService,
            cancellationToken);
    }

    public static IAsyncEnumerable<IAnnotatedSourceData<TData>> EnumerateEntitiesAnnotation<TDocument, TData>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService,
        CancellationToken cancellationToken
    )
        where TDocument : SourceDataDocumentBase
        where TData : notnull
    {
        var documents = documentQuery.Execute(mongoSession, DocumentBase.NoIdProjection, cancellationToken);

        return documents.EnumerateEntitiesAnnotation<TDocument, BsonDocument, TData>(envelopeService,
            cancellationToken);
    }
}
