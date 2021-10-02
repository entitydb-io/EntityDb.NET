using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal sealed record FactDocument
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        Guid EntityId,
        ulong EntityVersionNumber,
        ulong EntitySubversionNumber,
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

        private static readonly FactFilterBuilder _factFilterBuilder = new();
        private static readonly FactSortBuilder _factSortBuilder = new();
        private static readonly ProjectionDefinitionBuilder<BsonDocument> _projection = Builders<BsonDocument>.Projection;
        private static readonly IndexKeysDefinitionBuilder<BsonDocument> _indexKeys = Builders<BsonDocument>.IndexKeys;

        public const string CollectionName = "Facts";

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
                            _indexKeys.Ascending(nameof(EntityId)),
                            _indexKeys.Ascending(nameof(EntityVersionNumber)),
                            _indexKeys.Ascending(nameof(EntitySubversionNumber))
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

        public static async Task InsertMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            FactDocument[] factDocuments
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            await InsertMany
            (
                clientSessionHandle,
                mongoCollection,
                factDocuments
            );
        }

        public static async Task InsertMany<TEntity>
        (
            ILogger logger,
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            DateTime transactionTimeStamp,
            Guid transactionId,
            Guid entityId,
            ulong entityVersionNumber,
            ImmutableArray<ITransactionFact<TEntity>> transactionFacts
        )
        {
            if (transactionFacts.Length > 0)
            {
                var factDocuments = new List<FactDocument>();

                foreach (var transactionFact in transactionFacts)
                {
                    var factDocument = new FactDocument
                    (
                        transactionTimeStamp,
                        transactionId,
                        entityId,
                        entityVersionNumber,
                        transactionFact.SubversionNumber,
                        BsonDocumentEnvelope.Deconstruct(transactionFact.Fact, logger)
                    );

                    factDocuments.Add(factDocument);
                }

                await InsertMany
                (
                    clientSessionHandle,
                    mongoDatabase,
                    factDocuments.ToArray()
                );
            }
        }

        public static async Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = factQuery.GetFilter(_factFilterBuilder);
            var sort = factQuery.GetSort(_factSortBuilder);
            var skip = factQuery.Skip;
            var take = factQuery.Take;

            var factDocuments = await GetMany<FactDocument>
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

            var transactionIds = factDocuments
                .Select(factDocument => factDocument.TransactionId)
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
            IFactQuery factQuery
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = factQuery.GetFilter(_factFilterBuilder);
            var sort = factQuery.GetSort(_factSortBuilder);
            var skip = factQuery.Skip;
            var take = factQuery.Take;

            var factDocuments = await GetMany<FactDocument>
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

            var entityIds = factDocuments
                .Select(factDocument => factDocument.EntityId)
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

        public static async Task<IFact<TEntity>[]> GetFacts<TEntity>
        (
            ILogger logger,
            IResolvingStrategyChain resolvingStrategyChain,
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = factQuery.GetFilter(_factFilterBuilder);
            var sort = factQuery.GetSort(_factSortBuilder);
            var skip = factQuery.Skip;
            var take = factQuery.Take;

            var factDocuments = await GetMany<FactDocument>
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

            return factDocuments
                .Select(factDocument => factDocument.Data.Reconstruct<IFact<TEntity>>(logger, resolvingStrategyChain))
                .ToArray();
        }
    }
}
