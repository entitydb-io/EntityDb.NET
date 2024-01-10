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

internal sealed record TagDataDocument : MessageDataDocumentBase
{
    public const string CollectionName = "Tags";

    private static readonly TagDataFilterBuilder DataFilterBuilder = new();

    private static readonly TagSortBuilder SortBuilder = new();

    public required string Label { get; init; }
    public required string Value { get; init; }

    public static InsertDocumentsCommand<TagDataDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        Source source,
        Message message
    )
    {
        var tagDocuments = message.AddTags
            .Select(tag => new TagDataDocument
            {
                SourceTimeStamp = source.TimeStamp,
                SourceId = source.Id,
                MessageId = message.Id,
                StateId = message.StatePointer.Id,
                StateVersion = message.StatePointer.Version,
                StatePointer = message.StatePointer,
                DataType = tag.GetType().Name,
                Data = envelopeService.Serialize(tag),
                Label = tag.Label,
                Value = tag.Value,
            })
            .ToArray();

        return new InsertDocumentsCommand<TagDataDocument>
        (
            CollectionName,
            tagDocuments
        );
    }

    public static DocumentQuery<TagDataDocument> GetQuery
    (
        ITagQuery tagQuery
    )
    {
        return new DocumentQuery<TagDataDocument>
        {
            CollectionName = CollectionName,
            Filter = tagQuery.GetFilter(DataFilterBuilder),
            Sort = tagQuery.GetSort(SortBuilder),
            Skip = tagQuery.Skip,
            Limit = tagQuery.Take,
            Options = tagQuery.Options as MongoDbQueryOptions,
        };
    }

    public static DeleteDocumentsCommand GetDeleteCommand
    (
        Message message
    )
    {
        var deleteTagsQuery = new DeleteTagsQuery(message.StatePointer.Id, message.DeleteTags);

        return new DeleteDocumentsCommand
        {
            CollectionName = CollectionName, FilterDefinition = deleteTagsQuery.GetFilter(DataFilterBuilder),
        };
    }
}
