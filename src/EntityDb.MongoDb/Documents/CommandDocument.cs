using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal sealed record CommandDocument
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        Guid EntityId,
        ulong EntityVersionNumber,
        BsonDocumentEnvelope Data,
        ObjectId? _id = null
    ) : DocumentBase
    (
        TransactionTimeStamp,
        TransactionId,
        Data,
        _id
    ), IEntityDocument
    {
        private static readonly CommandFilterBuilder _commandFilterBuilder = new();
        private static readonly CommandSortBuilder _commandSortBuilder = new();

        public const string CollectionName = "Commands";

        public static Task ProvisionCollection
        (
            IMongoDatabase mongoDatabase
        )
        {
            return ProvisionCollection
            (
                mongoDatabase,
                CollectionName,
                new[]
                {
                    new CreateIndexModel<BsonDocument>
                    (
                        keys: IndexKeys.Combine
                        (
                            IndexKeys.Descending(nameof(EntityId)),
                            IndexKeys.Descending(nameof(EntityVersionNumber))
                        ),
                        options: new CreateIndexOptions
                        {
                            Name = $"Uniqueness Constraint",
                            Unique = true,
                        }
                    ),
                }
            );
        }

        public static CommandDocument BuildOne<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return new CommandDocument
            (
                transaction.TimeStamp,
                transaction.Id,
                transactionCommand.EntityId,
                transactionCommand.ExpectedPreviousVersionNumber + 1,
                BsonDocumentEnvelope.Deconstruct(transactionCommand.Command, logger)
            );
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
