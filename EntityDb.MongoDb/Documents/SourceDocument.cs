using EntityDb.Abstractions.Queries;
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
    internal sealed record SourceDocument
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        Guid[] EntityIds,
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
        private static readonly SourceFilterBuilder _sourceFilterBuilder = new();
        private static readonly SourceSortBuilder _sourceSortBuilder = new();
        private static readonly ProjectionDefinitionBuilder<BsonDocument> _projection = Builders<BsonDocument>.Projection;
        private static readonly IndexKeysDefinitionBuilder<BsonDocument> _indexKeys = Builders<BsonDocument>.IndexKeys;

        public const string CollectionName = "Sources";

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
                            _indexKeys.Descending(nameof(TransactionId))
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

        public static async Task InsertOne
        (
            IServiceProvider serviceProvider,
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            DateTime transactionTimeStamp,
            Guid transactionId,
            Guid[] entityIds,
            object source
        )
        {
            var sourceDocument = new SourceDocument
            (
                transactionTimeStamp,
                transactionId,
                entityIds,
                BsonDocumentEnvelope.Deconstruct(source, serviceProvider)
            );

            var mongoCollection = GetCollection(mongoDatabase);

            await InsertOne
            (
                clientSessionHandle,
                mongoCollection,
                sourceDocument
            );
        }

        public static async Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ISourceQuery sourceQuery
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = sourceQuery.GetFilter(_sourceFilterBuilder);
            var sort = sourceQuery.GetSort(_sourceSortBuilder);
            var skip = sourceQuery.Skip;
            var take = sourceQuery.Take;

            var sourceDocuments = await GetMany<SourceDocument>
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

            var transactionIds = sourceDocuments
                .Select(sourceDocument => sourceDocument.TransactionId)
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
            ISourceQuery sourceQuery
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = sourceQuery.GetFilter(_sourceFilterBuilder);
            var sort = sourceQuery.GetSort(_sourceSortBuilder);
            var skip = sourceQuery.Skip;
            var take = sourceQuery.Take;

            var sourceDocuments = await GetMany<SourceDocument>
            (
                clientSessionHandle,
                mongoCollection,
                filter,
                sort,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(EntityIds))
                ),
                null,
                null
            );

            var entityIds = sourceDocuments
                .SelectMany(sourceDocument => sourceDocument.EntityIds)
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

        public static async Task<object[]> GetSources
        (
            IServiceProvider serviceProvider,
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ISourceQuery sourceQuery
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = sourceQuery.GetFilter(_sourceFilterBuilder);
            var sort = sourceQuery.GetSort(_sourceSortBuilder);
            var skip = sourceQuery.Skip;
            var take = sourceQuery.Take;

            var sourceDocuments = await GetMany<SourceDocument>
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

            return sourceDocuments
                .Select(sourceDocument => sourceDocument.Data.Reconstruct<object>(serviceProvider))
                .ToArray();
        }
    }
}
