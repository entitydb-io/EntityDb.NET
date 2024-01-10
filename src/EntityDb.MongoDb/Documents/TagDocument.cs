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

internal sealed record TagDocument : MessageDocumentBase
{
    public const string CollectionName = "Tags";

    private static readonly TagFilterBuilder FilterBuilder = new();

    private static readonly TagSortBuilder SortBuilder = new();

    public required string Label { get; init; }
    public required string Value { get; init; }

    public static InsertDocumentsCommand<TagDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        Source source,
        Message message
    )
    {
        var tagDocuments = message.AddTags
            .Select(tag => new TagDocument
            {
                SourceTimeStamp = source.TimeStamp,
                SourceId = source.Id,
                MessageId = message.Id,
                EntityId = message.EntityPointer.Id,
                EntityVersion = message.EntityPointer.Version,
                EntityPointer = message.EntityPointer,
                DataType = tag.GetType().Name,
                Data = envelopeService.Serialize(tag),
                Label = tag.Label,
                Value = tag.Value,
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
        Message message
    )
    {
        var deleteTagsQuery = new DeleteTagsQuery(message.EntityPointer.Id, message.DeleteTags);

        return new DeleteDocumentsCommand
        (
            CollectionName,
            deleteTagsQuery.GetFilter(FilterBuilder)
        );
    }
}
