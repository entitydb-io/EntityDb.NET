using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal sealed record SourceDocument : DocumentBase, IEntitiesDocument
    {
        public const string CollectionName = "Sources";

        private static readonly SourceFilterBuilder _filterBuilder = new();

        private static readonly SourceSortBuilder _sortBuilder = new();

        public Guid[] EntityIds { get; init; } = default!;

        public static SourceDocument BuildOne<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction
        )
        {
            return new SourceDocument
            {
                TransactionTimeStamp = transaction.TimeStamp,
                TransactionId = transaction.Id,
                EntityIds = transaction.Steps.Select(command => command.EntityId).Distinct().ToArray(),
                Data = BsonDocumentEnvelope.Deconstruct(transaction.Source, logger)
            };
        }

        public static Task InsertOne<TEntity>
        (
            IMongoSession mongoSession,
            IMongoDatabase mongoDatabase,
            ILogger logger,
            ITransaction<TEntity> transaction
        )
        {
            return InsertOne
            (
                mongoSession,
                GetMongoCollection(mongoDatabase, CollectionName),
                BuildOne(logger, transaction)
            );
        }

        public static DocumentQuery<SourceDocument> GetDocumentQuery
        (
            IMongoSession? mongoSession,
            IMongoDatabase mongoDatabase,
            ISourceQuery sourceQuery
        )
        {
            return new DocumentQuery<SourceDocument>
            (
                MongoSession: mongoSession,
                MongoCollection: GetMongoCollection(mongoDatabase, CollectionName),
                Filter: sourceQuery.GetFilter(_filterBuilder),
                Sort: sourceQuery.GetSort(_sortBuilder),
                Skip: sourceQuery.Skip,
                Limit: sourceQuery.Take
            );
        }
    }
}
