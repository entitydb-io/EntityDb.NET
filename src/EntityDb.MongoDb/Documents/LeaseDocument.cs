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

internal sealed record LeaseDocument : MessageDocumentBase
{
    public const string CollectionName = "Leases";

    private static readonly LeaseFilterBuilder FilterBuilder = new();

    private static readonly LeaseSortBuilder SortBuilder = new();

    public required string Scope { get; init; }
    public required string Label { get; init; }
    public required string Value { get; init; }

    public static InsertDocumentsCommand<LeaseDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        Source source,
        Message message
    )
    {
        var leaseDocuments = message.AddLeases
            .Select(lease => new LeaseDocument
            {
                SourceTimeStamp = source.TimeStamp,
                SourceId = source.Id,
                EntityId = message.EntityPointer.Id,
                EntityVersion = message.EntityPointer.Version,
                EntityPointer = message.EntityPointer,
                DataType = lease.GetType().Name,
                Data = envelopeService.Serialize(lease),
                Scope = lease.Scope,
                Label = lease.Label,
                Value = lease.Value,
            })
            .ToArray();

        return new InsertDocumentsCommand<LeaseDocument>
        (
            CollectionName,
            leaseDocuments
        );
    }

    public static DocumentQuery<LeaseDocument> GetQuery
    (
        ILeaseQuery leaseQuery
    )
    {
        return new DocumentQuery<LeaseDocument>
        (
            CollectionName,
            leaseQuery.GetFilter(FilterBuilder),
            leaseQuery.GetSort(SortBuilder),
            leaseQuery.Skip,
            leaseQuery.Take,
            leaseQuery.Options as MongoDbQueryOptions
        );
    }

    public static DeleteDocumentsCommand GetDeleteCommand
    (
        Message message
    )
    {
        var deleteLeasesQuery = new DeleteLeasesQuery(message.DeleteLeases);

        return new DeleteDocumentsCommand
        (
            CollectionName,
            deleteLeasesQuery.GetFilter(FilterBuilder)
        );
    }
}
