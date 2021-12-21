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
    internal sealed record SourceDocument : DocumentBase, IEntitiesDocument
    {
        public const string CollectionName = "Sources";

        private static readonly SourceFilterBuilder _filterBuilder = new();

        private static readonly SourceSortBuilder _sortBuilder = new();

        public Guid[] EntityIds { get; init; } = default!;

        public static IReadOnlyCollection<SourceDocument> Build<TEntity>
        (
            ITransaction<TEntity> transaction,
            int transactionStepIndex,
            ILogger logger
        )
        {
            return new[]
            {
                new SourceDocument
                {
                    TransactionTimeStamp = transaction.TimeStamp,
                    TransactionId = transaction.Id,
                    EntityIds = transaction.Steps.Select(command => command.EntityId).Distinct().ToArray(),
                    Data = BsonDocumentEnvelope.Deconstruct(transaction.Source, logger)
                }
            };
        }

        public static InsertDocumentsCommand<TEntity, SourceDocument> GetInsertCommand<TEntity>
        (
            IMongoSession mongoSession
        )
        {
            return new InsertDocumentsCommand<TEntity, SourceDocument>
            (
                mongoSession,
                CollectionName,
                Build<TEntity>
            );
        }

        public static DocumentQuery<SourceDocument> GetQuery
        (
            IMongoSession mongoSession,
            ISourceQuery sourceQuery
        )
        {
            return new DocumentQuery<SourceDocument>
            (
                mongoSession,
                CollectionName,
                sourceQuery.GetFilter(_filterBuilder),
                sourceQuery.GetSort(_sortBuilder),
                sourceQuery.Skip,
                sourceQuery.Take
            );
        }
    }
}
