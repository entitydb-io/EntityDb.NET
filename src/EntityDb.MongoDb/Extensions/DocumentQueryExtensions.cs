using EntityDb.Abstractions.Annotations;
using EntityDb.Common.Annotations;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Queries;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    private static readonly ProjectionDefinition<BsonDocument> EntityVersionNumberProjection =
        ProjectionBuilder.Combine
        (
            ProjectionBuilder.Exclude(nameof(IEntityDocument._id)),
            ProjectionBuilder.Include(nameof(IEntityDocument.EntityVersionNumber))
        );

    public static async Task<ulong> GetEntityVersionNumber<TDocument>(this DocumentQuery<TDocument> documentQuery)
        where TDocument : IEntityDocument
    {
        var documents = await documentQuery.Execute(EntityVersionNumberProjection);

        var document = documents.SingleOrDefault();

        return document == null
            ? default
            : document.EntityVersionNumber;
    }

    private static async Task<Guid[]> GetGuids<TDocument>
    (
        this DocumentQuery<TDocument> documentQuery,
        ProjectionDefinition<BsonDocument, TDocument> projection,
        Func<List<TDocument>, IEnumerable<Guid>> mapToGuids
    )
    {
        var skip = documentQuery.Skip;
        var limit = documentQuery.Limit;

        documentQuery = documentQuery with { Skip = null, Limit = null };

        var documents = await documentQuery.Execute(projection);

        var guids = mapToGuids
            .Invoke(documents)
            .Distinct();

        if (skip.HasValue)
        {
            guids = guids.Skip(skip.Value);
        }

        if (limit.HasValue)
        {
            guids = guids.Take(limit.Value);
        }

        return guids.ToArray();
    }

    public static async Task<IEntityAnnotation<TData>[]> GetEntityAnnotation<TDocument, TData>(this DocumentQuery<TDocument> documentQuery)
        where TDocument : IEntityDocument
    {
        var documents = await documentQuery.Execute(NoDocumentIdProjection);

        return documents
            .Select(document => new EntityAnnotation<TData>
            (
                document.TransactionId,
                document.TransactionTimeStamp,
                document.EntityId,
                document.EntityVersionNumber,
                document.Data.Reconstruct<TData>(documentQuery.MongoSession.Logger, documentQuery.MongoSession.TypeResolver)
            ))
            .ToArray<IEntityAnnotation<TData>>();
    }

    public static async Task<TData[]> GetData<TDocument, TData>(this DocumentQuery<TDocument> documentQuery)
        where TDocument : ITransactionDocument
    {
        var documents = await documentQuery.Execute(DataProjection);

        return documents
            .Select(document => document.Data.Reconstruct<TData>(documentQuery.MongoSession.Logger, documentQuery.MongoSession.TypeResolver))
            .ToArray();
    }

    public static Task<Guid[]> GetEntityIds<TDocument>(this DocumentQuery<TDocument> documentQuery)
        where TDocument : IEntityDocument
    {
        return documentQuery.GetGuids
        (
            EntityIdProjection,
            documents => documents.Select(document => document.EntityId)
        );
    }

    public static Task<Guid[]> GetEntitiesIds<TDocument>(this DocumentQuery<TDocument> documentQuery)
        where TDocument : IEntitiesDocument
    {
        return documentQuery.GetGuids
        (
            EntityIdsProjection,
            documents => documents.SelectMany(document => document.EntityIds)
        );
    }

    public static Task<Guid[]> GetTransactionIds<TDocument>(this DocumentQuery<TDocument> documentQuery)
        where TDocument : ITransactionDocument
    {
        return documentQuery.GetGuids
        (
            TransactionIdProjection,
            documents => documents.Select(document => document.TransactionId)
        );
    }
}
