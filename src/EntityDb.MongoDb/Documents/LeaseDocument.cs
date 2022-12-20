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
        ITransactionCommand transactionCommand,
        IAddLeasesCommand addLeasesCommand
    )
    {
        var leaseDocuments = addLeasesCommand.GetLeases()
            .Select(insertLease => new LeaseDocument
            {
                TransactionTimeStamp = transaction.TimeStamp,
                TransactionId = transaction.Id,
                EntityId = transactionCommand.EntityId,
                EntityVersionNumber = transactionCommand.EntityVersionNumber,
                DataType = insertLease.GetType().Name,
                Data = envelopeService.Serialize(insertLease),
                Scope = insertLease.Scope,
                Label = insertLease.Label,
                Value = insertLease.Value
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
        ITransactionCommand transactionCommand,
        IDeleteLeasesCommand deleteLeasesCommand
    )
    {
        var deleteLeasesQuery =
            new DeleteLeasesQuery(transactionCommand.EntityId, deleteLeasesCommand.GetLeases().ToArray());

        return new DeleteDocumentsCommand
        (
            CollectionName,
            deleteLeasesQuery.GetFilter(FilterBuilder)
        );
    }
}
