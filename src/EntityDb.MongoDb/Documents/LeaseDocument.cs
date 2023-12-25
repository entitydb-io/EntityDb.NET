using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Commands;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Documents;

internal sealed record LeaseDocument : DocumentBase, IEntityDocument
{
    public const string CollectionName = "Leases";

    private static readonly LeaseFilterBuilder FilterBuilder = new();

    private static readonly LeaseSortBuilder SortBuilder = new();

    public string Scope { get; init; } = default!;
    public string Label { get; init; } = default!;
    public string Value { get; init; } = default!;
    public Id EntityId { get; init; }
    public VersionNumber EntityVersionNumber { get; init; }

    public static InsertDocumentsCommand<LeaseDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        ITransaction transaction,
        ITransactionCommand transactionCommand
    )
    {
        var leaseDocuments = transactionCommand.AddLeases
            .Select(lease => new LeaseDocument
            {
                TransactionTimeStamp = transaction.TimeStamp,
                TransactionId = transaction.Id,
                EntityId = transactionCommand.EntityId,
                EntityVersionNumber = transactionCommand.EntityVersionNumber,
                DataType = lease.GetType().Name,
                Data = envelopeService.Serialize(lease),
                Scope = lease.Scope,
                Label = lease.Label,
                Value = lease.Value
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
        ITransactionCommand transactionCommand
    )
    {
        var deleteLeasesQuery =
            new DeleteLeasesQuery(transactionCommand.EntityId, transactionCommand.DeleteLeases);

        return new DeleteDocumentsCommand
        (
            CollectionName,
            deleteLeasesQuery.GetFilter(FilterBuilder)
        );
    }
}
