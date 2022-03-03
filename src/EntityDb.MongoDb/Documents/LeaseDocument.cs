using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Commands;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using System.Linq;

namespace EntityDb.MongoDb.Documents;

internal sealed record LeaseDocument : DocumentBase, IEntityDocument
{
    public const string CollectionName = "Leases";

    private static readonly LeaseFilterBuilder FilterBuilder = new();

    private static readonly LeaseSortBuilder SortBuilder = new();

    public static readonly string[] HoistedFieldNames = { nameof(Scope), nameof(Label), nameof(Value) };

    public string Scope { get; init; } = default!;
    public string Label { get; init; } = default!;
    public string Value { get; init; } = default!;
    public Id EntityId { get; init; }
    public VersionNumber EntityVersionNumber { get; init; }

    public static InsertDocumentsCommand<LeaseDocument> GetInsertCommand
    (
        IMongoSession mongoSession,
        ITransaction transaction,
        IAddLeasesTransactionStep addLeasesTransactionStep
    )
    {
        var leaseDocuments = addLeasesTransactionStep.Leases
            .Select(insertLease => new LeaseDocument
            {
                TransactionTimeStamp = transaction.TimeStamp,
                TransactionId = transaction.Id,
                EntityId = addLeasesTransactionStep.EntityId,
                EntityVersionNumber = addLeasesTransactionStep.EntityVersionNumber,
                Scope = insertLease.Scope,
                Label = insertLease.Label,
                Value = insertLease.Value,
                Data = BsonDocumentEnvelope.Deconstruct(insertLease, mongoSession.Logger)
            })
            .ToArray();
        
        return new InsertDocumentsCommand<LeaseDocument>
        (
            mongoSession,
            CollectionName,
            leaseDocuments
        );
    }

    public static DocumentQuery<LeaseDocument> GetQuery
    (
        IMongoSession mongoSession,
        ILeaseQuery leaseQuery
    )
    {
        return new DocumentQuery<LeaseDocument>
        (
            mongoSession,
            CollectionName,
            leaseQuery.GetFilter(FilterBuilder),
            leaseQuery.GetSort(SortBuilder),
            leaseQuery.Skip,
            leaseQuery.Take
        );
    }

    public static DeleteDocumentsCommand GetDeleteCommand
    (
        IMongoSession mongoSession,
        IDeleteLeasesTransactionStep deleteLeasesTransactionStep
    )
    {
        var deleteLeasesQuery =
            new DeleteLeasesQuery(deleteLeasesTransactionStep.EntityId, deleteLeasesTransactionStep.Leases);
        
        return new DeleteDocumentsCommand
        (
            mongoSession,
            CollectionName,
            deleteLeasesQuery.GetFilter(FilterBuilder)
        );
    }
}
