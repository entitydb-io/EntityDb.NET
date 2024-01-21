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

    private static readonly TagDataSortBuilder DataSortBuilder = new();

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
                StateVersion = message.StatePointer.StateVersion,
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
        ITagDataQuery tagDataQuery
    )
    {
        return new DocumentQuery<TagDataDocument>
        {
            CollectionName = CollectionName,
            Filter = tagDataQuery.GetFilter(DataFilterBuilder),
            Sort = tagDataQuery.GetSort(DataSortBuilder),
            Skip = tagDataQuery.Skip,
            Limit = tagDataQuery.Take,
            Options = tagDataQuery.Options as MongoDbQueryOptions,
        };
    }

    public static DeleteDocumentsCommand GetDeleteCommand
    (
        Message message
    )
    {
        var deleteTagsQuery = new DeleteTagsDataQuery(message.StatePointer.Id, message.DeleteTags);

        return new DeleteDocumentsCommand
        {
            CollectionName = CollectionName, FilterDefinition = deleteTagsQuery.GetFilter(DataFilterBuilder),
        };
    }
}
