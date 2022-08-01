using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Commands;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using MongoDB.Bson;

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
        IEnvelopeService<BsonDocument> envelopeService,
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
                Data = envelopeService.Serialize(insertTag)
            })
            .ToArray();

        return new InsertDocumentsCommand<TagDocument>
        (
            CollectionName,
            tagDocuments
        );
    }

    public static DocumentQuery<TagDocument> GetQuery
    (
        ITagQuery tagQuery
    )
    {
        return new DocumentQuery<TagDocument>
        (
            CollectionName,
            tagQuery.GetFilter(FilterBuilder),
            tagQuery.GetSort(SortBuilder),
            tagQuery.Skip,
            tagQuery.Take,
            tagQuery.Options as MongoDbQueryOptions
        );
    }

    public static DeleteDocumentsCommand GetDeleteCommand
    (
        IDeleteTagsTransactionStep deleteTagsTransactionStep
    )
    {
        var deleteTagsQuery = new DeleteTagsQuery(deleteTagsTransactionStep.EntityId, deleteTagsTransactionStep.Tags);

        return new DeleteDocumentsCommand
        (
            CollectionName,
            deleteTagsQuery.GetFilter(FilterBuilder)
        );
    }
}
