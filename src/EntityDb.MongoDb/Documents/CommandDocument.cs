using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Commands;
using EntityDb.Common.Queries;
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
    internal sealed record CommandDocument : DocumentBase, IEntityDocument
    {
        public const string CollectionName = "Commands";

        private static readonly CommandFilterBuilder _commandFilterBuilder = new();
        private static readonly CommandSortBuilder _commandSortBuilder = new();
        public Guid EntityId { get; init; }
        public ulong EntityVersionNumber { get; init; }

        private static IAnnotatedCommand<TEntity> ToAnnotatedCommand<TEntity>
        (
            CommandDocument commandDocument,
            ILogger logger,
            IResolvingStrategyChain resolvingStrategyChain
        )
        {
            return new AnnotatedCommand<TEntity>
            {
                TransactionId = commandDocument.TransactionId,
                TransactionTimeStamp = commandDocument.TransactionTimeStamp,
                EntityId = commandDocument.EntityId,
                EntityVersionNumber = commandDocument.EntityVersionNumber,
                Command = commandDocument.Data.Reconstruct<ICommand<TEntity>>(logger, resolvingStrategyChain),
            };
        }

        public static CommandDocument BuildOne<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return new CommandDocument
            {
                TransactionTimeStamp = transaction.TimeStamp,
                TransactionId = transaction.Id,
                EntityId = transactionCommand.EntityId,
                EntityVersionNumber = transactionCommand.NextEntityVersionNumber,
                Data = BsonDocumentEnvelope.Deconstruct(transactionCommand.Command, logger)
            };
        }

        public static Task InsertOne
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            CommandDocument commandDocument
        )
        {
            return InsertOne
            (
                clientSessionHandle,
                GetMongoCollection(mongoDatabase, CollectionName),
                commandDocument
            );
        }

        public static Task<Guid[]> GetTransactionIds
        (
            IMongoDbSession mongoDbSession,
            ICommandQuery commandQuery
        )
        {
            return mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => new TransactionIdQuery<CommandDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = commandQuery.GetFilter(_commandFilterBuilder),
                    Sort = commandQuery.GetSort(_commandSortBuilder),
                    DistinctSkip = commandQuery.Skip,
                    DistinctLimit = commandQuery.Take
                }
            );
        }

        public static Task<Guid[]> GetEntityIds
        (
            IMongoDbSession mongoDbSession,
            ICommandQuery commandQuery
        )
        {
            return mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => new EntityIdQuery<CommandDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = commandQuery.GetFilter(_commandFilterBuilder),
                    Sort = commandQuery.GetSort(_commandSortBuilder),
                    DistinctSkip = commandQuery.Skip,
                    DistinctLimit = commandQuery.Take
                }
            );
        }

        public static Task<ICommand<TEntity>[]> GetData<TEntity>
        (
            IMongoDbSession mongoDbSession,
            ICommandQuery commandQuery
        )
        {
            return mongoDbSession.ExecuteDataQuery<CommandDocument, ICommand<TEntity>>
            (
                (clientSessionHandle, mongoDatabase) => new DataQuery<CommandDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = commandQuery.GetFilter(_commandFilterBuilder),
                    Sort = commandQuery.GetSort(_commandSortBuilder),
                    Skip = commandQuery.Skip,
                    Limit = commandQuery.Take
                }
            );
        }

        public static Task<IAnnotatedCommand<TEntity>[]> GetAnnotated<TEntity>
        (
            IMongoDbSession mongoDbSession,
            ICommandQuery commandQuery
        )
        {
            return mongoDbSession.ExecuteDocumentQuery
            (
                (clientSessionHandle, mongoDatabase) => new DocumentQuery<CommandDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = commandQuery.GetFilter(_commandFilterBuilder),
                    Sort = commandQuery.GetSort(_commandSortBuilder),
                    Skip = commandQuery.Skip,
                    Limit = commandQuery.Take
                },
                ToAnnotatedCommand<TEntity>
            );
        }

        public static async Task<ulong> GetLastEntityVersionNumber
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            Guid entityId
        )
        {
            var commandQuery = new GetLastEntityVersionQuery(entityId);

            var query = new EntityVersionQuery<CommandDocument>
            {
                ClientSessionHandle = clientSessionHandle,
                MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                Filter = commandQuery.GetFilter(_commandFilterBuilder),
                Sort = commandQuery.GetSort(_commandSortBuilder),
                Skip = commandQuery.Skip,
                Limit = commandQuery.Take
            };

            var commandDocuments = await query.GetDocuments();

            var lastCommandDocument = commandDocuments.SingleOrDefault();

            if (lastCommandDocument == null)
            {
                return default;
            }

            return lastCommandDocument.EntityVersionNumber;
        }
    }
}
