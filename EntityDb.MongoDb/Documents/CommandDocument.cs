using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
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
    )
    {
        private static readonly CommandFilterBuilder _commandFilterBuilder = new();
        private static readonly CommandSortBuilder _commandSortBuilder = new();
        private static readonly ProjectionDefinitionBuilder<BsonDocument> _projection = Builders<BsonDocument>.Projection;
        private static readonly IndexKeysDefinitionBuilder<BsonDocument> _indexKeys = Builders<BsonDocument>.IndexKeys;

        public const string CollectionName = "Commands";

        private static IMongoCollection<BsonDocument> GetCollection(IMongoDatabase mongoDatabase)
        {
            return mongoDatabase.GetCollection<BsonDocument>(CollectionName);
        }

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
                        keys: _indexKeys.Combine
                        (
                            _indexKeys.Descending(nameof(EntityId)),
                            _indexKeys.Descending(nameof(EntityVersionNumber))
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

        public static async Task InsertOne<TEntity>
        (
            ILogger logger,
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            DateTime transactionTimeStamp,
            Guid transactionId,
            Guid entityId,
            ulong entityVersionNumber,
            ICommand<TEntity> command
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var commandDocument = new CommandDocument
            (
                transactionTimeStamp,
                transactionId,
                entityId,
                entityVersionNumber,
                BsonDocumentEnvelope.Deconstruct(command, logger)
            );

            await InsertOne
            (
                clientSessionHandle,
                mongoCollection,
                commandDocument
            );
        }

        public static async Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ICommandQuery commandQuery
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = commandQuery.GetFilter(_commandFilterBuilder);
            var sort = commandQuery.GetSort(_commandSortBuilder);
            var skip = commandQuery.Skip;
            var take = commandQuery.Take;

            var commandDocuments = await GetMany<CommandDocument>
            (
                clientSessionHandle,
                mongoCollection,
                filter,
                sort,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(TransactionId))
                ),
                null,
                null
            );

            var transactionIds = commandDocuments
                .Select(commandDocument => commandDocument.TransactionId)
                .Distinct();

            if (skip.HasValue)
            {
                transactionIds = transactionIds.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                transactionIds = transactionIds.Take(take.Value);
            }

            return transactionIds.ToArray();
        }

        public static async Task<Guid[]> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ICommandQuery commandQuery
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = commandQuery.GetFilter(_commandFilterBuilder);
            var sort = commandQuery.GetSort(_commandSortBuilder);
            var skip = commandQuery.Skip;
            var take = commandQuery.Take;

            var commandDocuments = await GetMany<CommandDocument>
            (
                clientSessionHandle,
                mongoCollection,
                filter,
                sort,
                 _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(EntityId))
                ),
                null,
                null
            );

            var entityIds = commandDocuments
                .Select(commandDocument => commandDocument.EntityId)
                .Distinct();

            if (skip.HasValue)
            {
                entityIds = entityIds.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                entityIds = entityIds.Take(take.Value);
            }

            return entityIds.ToArray();
        }

        public static async Task<ICommand<TEntity>[]> GetCommands<TEntity>
        (
            ILogger logger,
            IResolvingStrategyChain resolvingStrategyChain,
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ICommandQuery commandQuery
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = commandQuery.GetFilter(_commandFilterBuilder);
            var sort = commandQuery.GetSort(_commandSortBuilder);
            var skip = commandQuery.Skip;
            var take = commandQuery.Take;

            var commandDocuments = await GetMany<CommandDocument>
            (
                clientSessionHandle,
                mongoCollection,
                filter,
                sort,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(Data))
                ),
                skip,
                take
            );

            return commandDocuments
                .Select(commandDocument => commandDocument.Data.Reconstruct<ICommand<TEntity>>(logger, resolvingStrategyChain))
                .ToArray();
        }

        public static async Task<ulong> GetPreviousVersionNumber
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            Guid entityId
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var commandQuery = new GetLastCommandQuery(entityId);

            var filter = commandQuery.GetFilter(_commandFilterBuilder);
            var sort = commandQuery.GetSort(_commandSortBuilder);
            var skip = commandQuery.Skip;
            var take = commandQuery.Take;

            var commandDocuments = await GetMany<CommandDocument>
            (
                clientSessionHandle,
                mongoCollection,
                filter,
                sort,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(EntityVersionNumber))
                ),
                skip,
                take
            );

            var lastCommandDocument = commandDocuments.SingleOrDefault();

            if (lastCommandDocument != null)
            {
                return lastCommandDocument.EntityVersionNumber;
            }

            return 0;
        }
    }
}
