using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Common.Envelopes;
using EntityDb.MongoDb.Documents.Commands;
using EntityDb.MongoDb.Documents.Queries;
using EntityDb.MongoDb.Sources.Queries;
using EntityDb.MongoDb.Sources.Queries.SortBuilders;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Documents;

internal sealed record AgentSignatureDocument : SourceDataDocumentBase
{
    public const string CollectionName = "AgentSignatures";

    private static readonly SourceDataSortBuilder SortBuilder = new();

    public static InsertDocumentsCommand<AgentSignatureDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        Source source
    )
    {
        var messageIds = source.Messages
            .Select(message => message.Id)
            .ToArray();

        var statePointers = source.Messages
            .Select(message => message.StatePointer)
            .Distinct()
            .ToArray();

        var stateIds = statePointers
            .Select(statePointer => statePointer.Id)
            .ToArray();

        var documents = new[]
        {
            new AgentSignatureDocument
            {
                SourceTimeStamp = source.TimeStamp,
                SourceId = source.Id,
                MessageIds = messageIds,
                StateIds = stateIds,
                StatePointers = statePointers,
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
        ISourceDataQuery sourceDataQuery
    )
    {
        return new DocumentQuery<AgentSignatureDocument>
        {
            CollectionName = CollectionName,
            Filter = sourceDataQuery.GetFilter(FilterBuilder),
            Sort = sourceDataQuery.GetSort(SortBuilder),
            Skip = sourceDataQuery.Skip,
            Limit = sourceDataQuery.Take,
            Options = sourceDataQuery.Options as MongoDbQueryOptions,
        };
    }
}
