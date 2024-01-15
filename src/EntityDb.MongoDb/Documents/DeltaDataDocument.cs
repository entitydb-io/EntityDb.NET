using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.States;
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

namespace EntityDb.MongoDb.Documents;

internal sealed record DeltaDataDocument : MessageDataDocumentBase
{
    public const string CollectionName = "Deltas";

    private static readonly ProjectionDefinition<BsonDocument> StateVersionProjection =
        ProjectionBuilder.Combine
        (
            ProjectionBuilder.Exclude(nameof(_id)),
            ProjectionBuilder.Include(nameof(StateVersion))
        );

    private static readonly MessageDataFilterBuilder DataFilterBuilder = new();

    private static readonly MessageDataSortBuilder DataSortBuilder = new();

    public static InsertDocumentsCommand<DeltaDataDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        Source source,
        Message message
    )
    {
        var documents = new[]
        {
            new DeltaDataDocument
            {
                SourceTimeStamp = source.TimeStamp,
                SourceId = source.Id,
                MessageId = message.Id,
                StateId = message.StatePointer.Id,
                StateVersion = message.StatePointer.StateVersion,
                StatePointer = message.StatePointer,
                DataType = message.Delta.GetType().Name,
                Data = envelopeService.Serialize(message.Delta),
            },
        };

        return new InsertDocumentsCommand<DeltaDataDocument>
        (
            CollectionName,
            documents
        );
    }

    public static DocumentQuery<DeltaDataDocument> GetQuery
    (
        IMessageDataQuery messageDataQuery
    )
    {
        return new DocumentQuery<DeltaDataDocument>
        {
            CollectionName = CollectionName,
            Filter = messageDataQuery.GetFilter(DataFilterBuilder),
            Sort = messageDataQuery.GetSort(DataSortBuilder),
            Skip = messageDataQuery.Skip,
            Limit = messageDataQuery.Take,
            Options = messageDataQuery.Options as MongoDbQueryOptions,
        };
    }

    public static async Task<StateVersion> GetLastStateVersion
    (
        IMongoSession mongoSession,
        Id stateId,
        CancellationToken cancellationToken
    )
    {
        var lastStateVersionQuery = new GetLastStateVersionDataQuery(stateId);

        var document = await GetQuery(lastStateVersionQuery)
            .Execute(mongoSession, StateVersionProjection, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        return document?.StateVersion ?? default;
    }
}
