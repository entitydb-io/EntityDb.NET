using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.MongoDb.Documents.Commands;
using EntityDb.MongoDb.Documents.Queries;
using EntityDb.MongoDb.Sources.Queries;
using EntityDb.MongoDb.Sources.Queries.FilterBuilders;
using EntityDb.MongoDb.Sources.Queries.SortBuilders;
using EntityDb.MongoDb.Sources.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.MongoDb.Documents;

internal sealed record DeltaDocument : MessageDocumentBase
{
    public const string CollectionName = "Deltas";

    private static readonly ProjectionDefinition<BsonDocument> EntityVersionProjection =
        ProjectionBuilder.Combine
        (
            ProjectionBuilder.Exclude(nameof(_id)),
            ProjectionBuilder.Include(nameof(EntityVersion))
        );

    private static readonly MessageFilterBuilder FilterBuilder = new();

    private static readonly MessageSortBuilder SortBuilder = new();

    public static InsertDocumentsCommand<DeltaDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        Source source,
        Message message
    )
    {
        var documents = new[]
        {
            new DeltaDocument
            {
                SourceTimeStamp = source.TimeStamp,
                SourceId = source.Id,
                MessageId = message.Id,
                EntityId = message.EntityPointer.Id,
                EntityVersion = message.EntityPointer.Version,
                EntityPointer = message.EntityPointer,
                DataType = message.Delta.GetType().Name,
                Data = envelopeService.Serialize(message.Delta),
            },
        };

        return new InsertDocumentsCommand<DeltaDocument>
        (
            CollectionName,
            documents
        );
    }

    public static DocumentQuery<DeltaDocument> GetQuery
    (
        IMessageQuery messageQuery
    )
    {
        return new DocumentQuery<DeltaDocument>
        (
            CollectionName,
            messageQuery.GetFilter(FilterBuilder),
            messageQuery.GetSort(SortBuilder),
            messageQuery.Skip,
            messageQuery.Take,
            messageQuery.Options as MongoDbQueryOptions
        );
    }

    public static async Task<Version> GetLastEntityVersion
    (
        IMongoSession mongoSession,
        Id entityId,
        CancellationToken cancellationToken
    )
    {
        var lastEntityVersionQuery = new GetLastEntityVersionQuery(entityId);

        var document = await GetQuery(lastEntityVersionQuery)
            .Execute(mongoSession, EntityVersionProjection, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        return document?.EntityVersion ?? default;
    }
}
