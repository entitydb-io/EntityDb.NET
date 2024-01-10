using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.MongoDb.Documents.Commands;
using EntityDb.MongoDb.Documents.Queries;
using EntityDb.MongoDb.Sources.Queries;
using EntityDb.MongoDb.Sources.Queries.FilterBuilders;
using EntityDb.MongoDb.Sources.Queries.SortBuilders;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Documents;

internal sealed record LeaseDataDocument : MessageDataDocumentBase
{
    public const string CollectionName = "Leases";

    private static readonly LeaseDataFilterBuilder DataFilterBuilder = new();

    private static readonly LeaseDataSortBuilder DataSortBuilder = new();

    public required string Scope { get; init; }
    public required string Label { get; init; }
    public required string Value { get; init; }

    public static InsertDocumentsCommand<LeaseDataDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        Source source,
        Message message
    )
    {
        var leaseDocuments = message.AddLeases
            .Select(lease => new LeaseDataDocument
            {
                SourceTimeStamp = source.TimeStamp,
                SourceId = source.Id,
                MessageId = message.Id,
                StateId = message.StatePointer.Id,
                StateVersion = message.StatePointer.Version,
                StatePointer = message.StatePointer,
                DataType = lease.GetType().Name,
                Data = envelopeService.Serialize(lease),
                Scope = lease.Scope,
                Label = lease.Label,
                Value = lease.Value,
            })
            .ToArray();

        return new InsertDocumentsCommand<LeaseDataDocument>
        (
            CollectionName,
            leaseDocuments
        );
    }

    public static DocumentQuery<LeaseDataDocument> GetQuery
    (
        ILeaseDataDataQuery leaseDataDataQuery
    )
    {
        return new DocumentQuery<LeaseDataDocument>
        {
            CollectionName = CollectionName,
            Filter = leaseDataDataQuery.GetFilter(DataFilterBuilder),
            Sort = leaseDataDataQuery.GetSort(DataSortBuilder),
            Skip = leaseDataDataQuery.Skip,
            Limit = leaseDataDataQuery.Take,
            Options = leaseDataDataQuery.Options as MongoDbQueryOptions,
        };
    }

    public static DeleteDocumentsCommand GetDeleteCommand
    (
        Message message
    )
    {
        var deleteLeasesQuery = new DeleteLeasesDataQuery(message.DeleteLeases);

        return new DeleteDocumentsCommand
        {
            CollectionName = CollectionName, FilterDefinition = deleteLeasesQuery.GetFilter(DataFilterBuilder),
        };
    }
}
