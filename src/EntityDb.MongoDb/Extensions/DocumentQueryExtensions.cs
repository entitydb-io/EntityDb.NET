﻿using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Extensions;
using EntityDb.Common.Polyfills;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace EntityDb.MongoDb.Extensions;

internal static class DocumentQueryExtensions
{
    private static readonly ProjectionDefinitionBuilder<BsonDocument> ProjectionBuilder =
        Builders<BsonDocument>.Projection;

    private static readonly ProjectionDefinition<BsonDocument> NoDocumentIdProjection =
        ProjectionBuilder.Exclude(nameof(DocumentBase._id));

    private static readonly ProjectionDefinition<BsonDocument> EntityIdProjection =
        ProjectionBuilder.Include(nameof(IEntityDocument.EntityId));

    private static readonly ProjectionDefinition<BsonDocument> EntityIdsProjection =
        ProjectionBuilder.Include(nameof(IEntitiesDocument.EntityIds));

    private static readonly ProjectionDefinition<BsonDocument> TransactionIdProjection =
        ProjectionBuilder.Include(nameof(ITransactionDocument.TransactionId));

    private static readonly ProjectionDefinition<BsonDocument> DataProjection =
        ProjectionBuilder.Combine
        (
            ProjectionBuilder.Exclude(nameof(ITransactionDocument._id)),
            ProjectionBuilder.Include(nameof(ITransactionDocument.Data))
        );

    public static readonly ProjectionDefinition<BsonDocument> EntityVersionNumberProjection =
        ProjectionBuilder.Combine
        (
            ProjectionBuilder.Exclude(nameof(IEntityDocument._id)),
            ProjectionBuilder.Include(nameof(IEntityDocument.EntityVersionNumber))
        );

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

    public static IAsyncEnumerable<Id> EnumerateTransactionIds<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        CancellationToken cancellationToken
    )
        where TDocument : ITransactionDocument
    {
        return documentQuery.EnumerateIds
        (
            mongoSession,
            TransactionIdProjection,
            documents => documents.Select(document => document.TransactionId),
            cancellationToken
        );
    }

    public static IAsyncEnumerable<Id> EnumerateEntitiesIds<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        CancellationToken cancellationToken
    )
        where TDocument : IEntitiesDocument
    {
        return documentQuery.EnumerateIds
        (
            mongoSession,
            EntityIdsProjection,
            documents => documents.SelectMany(document => AsyncEnumerablePolyfill.FromResult(document.EntityIds)),
            cancellationToken
        );
    }

    public static IAsyncEnumerable<Id> EnumerateEntityIds<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        CancellationToken cancellationToken
    )
        where TDocument : IEntityDocument
    {
        return documentQuery.EnumerateIds
        (
            mongoSession,
            EntityIdProjection,
            documents => documents.Select(document => document.EntityId),
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
        where TDocument : ITransactionDocument
    {
        var documents = documentQuery.Execute(mongoSession, DataProjection, cancellationToken);

        await foreach (var document in documents)
        {
            yield return envelopeService.Deserialize<TData>(document.Data);
        }
    }

    public static IAsyncEnumerable<IEntityAnnotation<TData>> EnumerateEntityAnnotation<TDocument, TData>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService,
        CancellationToken cancellationToken
    )
        where TDocument : IEntityDocument
        where TData : notnull
    {
        var documents = documentQuery.Execute(mongoSession, NoDocumentIdProjection, cancellationToken);

        return documents.EnumerateEntityAnnotation<TDocument, BsonDocument, TData>(envelopeService, cancellationToken);
    }

    public static IAsyncEnumerable<IEntitiesAnnotation<TData>> EnumerateEntitiesAnnotation<TDocument, TData>
    (
        this DocumentQuery<TDocument> documentQuery,
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService,
        CancellationToken cancellationToken
    )
        where TDocument : IEntitiesDocument
        where TData : notnull
    {
        var documents = documentQuery.Execute(mongoSession, NoDocumentIdProjection, cancellationToken);

        return documents.EnumerateEntitiesAnnotation<TDocument, BsonDocument, TData>(envelopeService, cancellationToken);
    }
}
