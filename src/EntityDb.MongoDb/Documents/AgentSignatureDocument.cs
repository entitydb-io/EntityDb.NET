using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Common.Envelopes;
using EntityDb.MongoDb.Documents.Commands;
using EntityDb.MongoDb.Documents.Queries;
using EntityDb.MongoDb.Sources.Queries;
using EntityDb.MongoDb.Sources.Queries.SortBuilders;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Documents;

internal sealed record AgentSignatureDocument : MessageGroupDocumentBase
{
    public const string CollectionName = "AgentSignatures";

    private static readonly MessageGroupSortBuilder SortBuilder = new();

    public static InsertDocumentsCommand<AgentSignatureDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        Source source
    )
    {
        var messageIds = source.Messages
            .Select(message => message.Id)
            .ToArray();
        
        var entityPointers = source.Messages
            .Select(message => message.EntityPointer)
            .Distinct()
            .ToArray();

        var entityIds = entityPointers
            .Select(entityPointer => entityPointer.Id)
            .ToArray();

        var documents = new[]
        {
            new AgentSignatureDocument
            {
                SourceTimeStamp = source.TimeStamp,
                SourceId = source.Id,
                MessageIds = messageIds,
                EntityIds = entityIds,
                EntityPointers = entityPointers,
                DataType = source.AgentSignature.GetType().Name,
                Data = envelopeService.Serialize(source.AgentSignature),
            },
        };

        return new InsertDocumentsCommand<AgentSignatureDocument>
        (
            CollectionName,
            documents
        );
    }

    public static DocumentQuery<AgentSignatureDocument> GetQuery
    (
        IMessageGroupQuery messageGroupQuery
    )
    {
        return new DocumentQuery<AgentSignatureDocument>
        (
            CollectionName,
            messageGroupQuery.GetFilter(FilterBuilder),
            messageGroupQuery.GetSort(SortBuilder),
            messageGroupQuery.Skip,
            messageGroupQuery.Take,
            messageGroupQuery.Options as MongoDbQueryOptions
        );
    }
}
