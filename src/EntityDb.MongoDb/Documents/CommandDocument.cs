using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal sealed record CommandDocument : DocumentBase, IEntityDocument
    {
        public const string CollectionName = "Commands";

        private static readonly CommandFilterBuilder _filterBuilder = new();

        private static readonly CommandSortBuilder _sortBuilder = new();

        public Guid EntityId { get; init; }
        public ulong EntityVersionNumber { get; init; }

        public static CommandDocument BuildOne<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionStep<TEntity> transactionStep
        )
        {
            return new CommandDocument
            {
                TransactionTimeStamp = transaction.TimeStamp,
                TransactionId = transaction.Id,
                EntityId = transactionStep.EntityId,
                EntityVersionNumber = transactionStep.NextEntityVersionNumber,
                Data = BsonDocumentEnvelope.Deconstruct(transactionStep.Command, logger)
            };
        }

        public static Task InsertOne<TEntity>
        (
            IMongoSession mongoSession,
            IMongoDatabase mongoDatabase,
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionStep<TEntity> transactionStep
        )
        {
            return InsertOne
            (
                mongoSession,
                GetMongoCollection(mongoDatabase, CollectionName),
                BuildOne(logger, transaction, transactionStep)
            );
        }

        public static DocumentQuery<CommandDocument> GetDocumentQuery
        (
            IMongoSession? mongoSession,
            IMongoDatabase mongoDatabase,
            ICommandQuery commandQuery
        )
        {
            return new DocumentQuery<CommandDocument>
            (
                MongoSession: mongoSession,
                MongoCollection: GetMongoCollection(mongoDatabase, CollectionName),
                Filter: commandQuery.GetFilter(_filterBuilder),
                Sort: commandQuery.GetSort(_sortBuilder),
                Skip: commandQuery.Skip,
                Limit: commandQuery.Take
            );
        }

        public static Task<ulong> GetLastEntityVersionNumber
        (
            IMongoSession mongoSession,
            IMongoDatabase mongoDatabase,
            Guid entityId
        )
        {
            var commandQuery = new GetLastEntityVersionQuery(entityId);

            var documentQuery = GetDocumentQuery
            (
                mongoSession,
                mongoDatabase,
                commandQuery
            );

            return documentQuery.GetEntityVersionNumber();
        }
    }
}
