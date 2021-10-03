using EntityDb.Abstractions.Queries;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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
    ) : TransactionDocumentBase
    (
        TransactionTimeStamp,
        TransactionId,
        Data,
        _id
    )
    {
        private static readonly SourceFilterBuilder _sourceFilterBuilder = new();
        private static readonly SourceSortBuilder _sourceSortBuilder = new();

        private static readonly ProjectionDefinition<BsonDocument> _entityIdsProjection = Projection.Combine
        (
            Projection.Exclude(nameof(_id)),
            Projection.Include(nameof(EntityIds))
        );

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
                        keys: IndexKeys.Combine
                        (
                            IndexKeys.Descending(nameof(TransactionId))
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

        public static Task InsertOne
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            SourceDocument sourceDocument
        )
        {
            return InsertOne
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                sourceDocument
            );
        }

        public static Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ISourceQuery sourceQuery
        )
        {
            return GetTransactionIds<SourceDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                sourceQuery.GetFilter(_sourceFilterBuilder),
                sourceQuery.GetSort(_sourceSortBuilder),
                sourceQuery.Skip,
                sourceQuery.Take
            );
        }

        public static async Task<Guid[]> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ISourceQuery sourceQuery
        )
        {
            var sourceDocuments = await GetMany<SourceDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                sourceQuery.GetFilter(_sourceFilterBuilder),
                sourceQuery.GetSort(_sourceSortBuilder),
                _entityIdsProjection
            );

            var entityIds = sourceDocuments
                .SelectMany(sourceDocument => sourceDocument.EntityIds)
                .Distinct();

            if (sourceQuery.Skip.HasValue)
            {
                entityIds = entityIds.Skip(sourceQuery.Skip.Value);
            }

            if (sourceQuery.Take.HasValue)
            {
                entityIds = entityIds.Take(sourceQuery.Take.Value);
            }

            return entityIds.ToArray();
        }

        public static Task<List<SourceDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ISourceQuery sourceQuery
        )
        {
            return GetMany<SourceDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                sourceQuery.GetFilter(_sourceFilterBuilder),
                sourceQuery.GetSort(_sourceSortBuilder),
                DataProjection,
                sourceQuery.Skip,
                sourceQuery.Take
            );
        }
    }
}
