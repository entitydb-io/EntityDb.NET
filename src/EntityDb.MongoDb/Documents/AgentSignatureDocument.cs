﻿using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.MongoDb.Commands;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Documents;

internal sealed record AgentSignatureDocument : DocumentBase, IEntitiesDocument
{
    public const string CollectionName = "AgentSignatures";

    private static readonly AgentSignatureFilterBuilder FilterBuilder = new();

    private static readonly AgentSignatureSortBuilder SortBuilder = new();

    public Id[] EntityIds { get; init; } = default!;

    public static InsertDocumentsCommand<AgentSignatureDocument> GetInsertCommand
    (
        IEnvelopeService<BsonDocument> envelopeService,
        ITransaction transaction
    )
    {
        var documents = new[]
        {
            new AgentSignatureDocument
            {
                TransactionTimeStamp = transaction.TimeStamp,
                TransactionId = transaction.Id,
                EntityIds = transaction.Steps.Select(transactionStep => transactionStep.EntityId).Distinct()
                    .ToArray(),
                DataType = transaction.AgentSignature.GetType().Name,
                Data = envelopeService.Serialize(transaction.AgentSignature)
            }
        };

        return new InsertDocumentsCommand<AgentSignatureDocument>
        (
            CollectionName,
            documents
        );
    }

    public static DocumentQuery<AgentSignatureDocument> GetQuery
    (
        IAgentSignatureQuery agentSignatureQuery
    )
    {
        return new DocumentQuery<AgentSignatureDocument>
        (
            CollectionName,
            agentSignatureQuery.GetFilter(FilterBuilder),
            agentSignatureQuery.GetSort(SortBuilder),
            agentSignatureQuery.Skip,
            agentSignatureQuery.Take,
            agentSignatureQuery.Options as MongoDbQueryOptions
        );
    }
}
