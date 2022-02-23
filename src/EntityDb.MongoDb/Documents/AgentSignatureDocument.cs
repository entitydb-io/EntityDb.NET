using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Commands;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.MongoDb.Documents
{
    internal sealed record AgentSignatureDocument : DocumentBase, IEntitiesDocument
    {
        public const string CollectionName = "AgentSignatures";

        private static readonly AgentSignatureFilterBuilder _filterBuilder = new();

        private static readonly AgentSignatureSortBuilder _sortBuilder = new();

        public Guid[] EntityIds { get; init; } = default!;

        public static IReadOnlyCollection<AgentSignatureDocument> Build<TEntity>
        (
            ITransaction<TEntity> transaction,
            ILogger logger
        )
        {
            return new[]
            {
                new AgentSignatureDocument
                {
                    TransactionTimeStamp = transaction.TimeStamp,
                    TransactionId = transaction.Id,
                    EntityIds = transaction.Steps.Select(command => command.EntityId).Distinct().ToArray(),
                    Data = BsonDocumentEnvelope.Deconstruct(transaction.AgentSignature, logger)
                }
            };
        }

        public static InsertDocumentCommand<TEntity, AgentSignatureDocument> GetInsertCommand<TEntity>
        (
            IMongoSession mongoSession
        )
        {
            return new InsertDocumentCommand<TEntity, AgentSignatureDocument>
            (
                mongoSession,
                CollectionName,
                Build<TEntity>
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
                agentSignatureQuery.GetFilter(_filterBuilder),
                agentSignatureQuery.GetSort(_sortBuilder),
                agentSignatureQuery.Skip,
                agentSignatureQuery.Take
            );
        }
    }
}
