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

internal sealed record TagDocument : DocumentBase, IEntityDocument
{
    public const string CollectionName = "Tags";

    private static readonly TagFilterBuilder FilterBuilder = new();

    private static readonly TagSortBuilder SortBuilder = new();

    public static readonly string[] HoistedFieldNames = { nameof(Label), nameof(Value) };

    public string Label { get; init; } = default!;
    public string Value { get; init; } = default!;
    public Id EntityId { get; init; }
    public VersionNumber EntityVersionNumber { get; init; }

    public static InsertDocumentsCommand<TagDocument> GetInsertCommand
    (
        IMongoSession mongoSession,
        ITransaction transaction,
        IAddTagsTransactionStep addTagsTransactionStep
    )
    {
        var tagDocuments = addTagsTransactionStep.Tags
            .Select(insertTag => new TagDocument
            {
                TransactionTimeStamp = transaction.TimeStamp,
                TransactionId = transaction.Id,
                EntityId = addTagsTransactionStep.EntityId,
                EntityVersionNumber = addTagsTransactionStep.EntityVersionNumber,
                Label = insertTag.Label,
                Value = insertTag.Value,
                Data = BsonDocumentEnvelope.Deconstruct(insertTag, mongoSession.Logger)
            })
            .ToArray();
        
        return new InsertDocumentsCommand<TagDocument>
        (
            mongoSession,
            CollectionName,
            tagDocuments
        );
    }

    public static DocumentQuery<TagDocument> GetQuery
    (
        IMongoSession mongoSession,
        ITagQuery tagQuery
    )
    {
        return new DocumentQuery<TagDocument>
        (
            mongoSession,
            CollectionName,
            tagQuery.GetFilter(FilterBuilder),
            tagQuery.GetSort(SortBuilder),
            tagQuery.Skip,
            tagQuery.Take
        );
    }

    public static DeleteDocumentsCommand GetDeleteCommand
    (
        IMongoSession mongoSession,
        IDeleteTagsTransactionStep deleteTagsTransactionStep
    )
    {
        var deleteTagsQuery = new DeleteTagsQuery(deleteTagsTransactionStep.EntityId, deleteTagsTransactionStep.Tags);

        return new DeleteDocumentsCommand
        (
            mongoSession,
            CollectionName,
            deleteTagsQuery.GetFilter(FilterBuilder)
        );
    }
}
