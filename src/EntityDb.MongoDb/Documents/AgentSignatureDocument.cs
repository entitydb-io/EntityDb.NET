using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Commands;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using System;
using System.Linq;

namespace EntityDb.MongoDb.Documents;

internal sealed record AgentSignatureDocument : DocumentBase, IEntitiesDocument
{
    public const string CollectionName = "AgentSignatures";

    private static readonly AgentSignatureFilterBuilder FilterBuilder = new();

    private static readonly AgentSignatureSortBuilder SortBuilder = new();

    public Guid[] EntityIds { get; init; } = default!;

    private static AgentSignatureDocument Build
    (
        ITransaction transaction,
        ILogger logger
    )
    {
        return new AgentSignatureDocument
        {
            TransactionTimeStamp = transaction.TimeStamp,
            TransactionId = transaction.Id,
            EntityIds = transaction.Steps.Select(transactionStep => transactionStep.EntityId).Distinct().ToArray(),
            Data = BsonDocumentEnvelope.Deconstruct(transaction.AgentSignature, logger)
        };
    }

    public static InsertDocumentCommand<AgentSignatureDocument> GetInsertCommand
    (
        IMongoSession mongoSession
    )
    {
        return new InsertDocumentCommand<AgentSignatureDocument>
        (
            mongoSession,
            CollectionName,
            Build
        );
    }

    public static DocumentQuery<AgentSignatureDocument> GetQuery
    (
        IMongoSession mongoSession,
        IAgentSignatureQuery agentSignatureQuery
    )
    {
        return new DocumentQuery<AgentSignatureDocument>
        (
            mongoSession,
            CollectionName,
            agentSignatureQuery.GetFilter(FilterBuilder),
            agentSignatureQuery.GetSort(SortBuilder),
            agentSignatureQuery.Skip,
            agentSignatureQuery.Take
        );
    }
}
