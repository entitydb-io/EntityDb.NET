using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.SqlDb.Commands;
using EntityDb.SqlDb.Queries;
using EntityDb.SqlDb.Queries.FilterBuilders;
using EntityDb.SqlDb.Queries.SortBuilders;

namespace EntityDb.SqlDb.Documents.AgentSignature;

internal sealed record AgentSignatureDocument : DocumentBase, IEntitiesDocument<AgentSignatureDocument>
{
    public static string TableName => "AgentSignatures";

    private static readonly AgentSignatureFilterBuilder FilterBuilder = new();

    private static readonly AgentSignatureSortBuilder SortBuilder = new();

    public Id[] EntityIds { get; init; } = Array.Empty<Id>();

    public static IDocumentReader<AgentSignatureDocument> DocumentReader { get; } = new AgentSignatureDocumentReader();

    public static IDocumentReader<AgentSignatureDocument> TransactionIdDocumentReader { get; } = new AgentSignatureTransactionIdDocumentReader();

    public static IDocumentReader<AgentSignatureDocument> DataDocumentReader { get; } = new AgentSignatureDataDocumentReader();

    public static IDocumentReader<AgentSignatureDocument> EntityIdsDocumentReader { get; } = new AgentSignatureEntityIdsDocumentReader();

    public static InsertDocumentsCommand<AgentSignatureDocument> GetInsert
    (
        IEnvelopeService<string> envelopeService,
        ITransaction transaction
    )
    {
        return new InsertDocumentsCommand<AgentSignatureDocument>
        (
            TableName,
            new[]
            {
                new AgentSignatureDocument
                {
                    TransactionTimeStamp = transaction.TimeStamp,
                    TransactionId = transaction.Id,
                    EntityIds = transaction.Steps
                        .Select(transactionStep => transactionStep.EntityId)
                        .Distinct()
                        .ToArray(),
                    DataType = transaction.AgentSignature.GetType().Name,
                    Data = envelopeService.Serialize(transaction.AgentSignature)
                }
            }
        );
    }

    public static DocumentQuery<AgentSignatureDocument> GetQuery
    (
        IAgentSignatureQuery agentSignatureQuery
    )
    {
        return new DocumentQuery<AgentSignatureDocument>
        (
            agentSignatureQuery.GetFilter(FilterBuilder),
            agentSignatureQuery.GetSort(SortBuilder),
            agentSignatureQuery.Skip,
            agentSignatureQuery.Take,
            agentSignatureQuery.Options
        );
    }

    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            [nameof(TransactionId)] = TransactionId,
            [nameof(TransactionTimeStamp)] = TransactionTimeStamp,
            [nameof(EntityIds)] = EntityIds,
            [nameof(DataType)] = DataType,
            [nameof(Data)] = Data,
        };
    }
}
