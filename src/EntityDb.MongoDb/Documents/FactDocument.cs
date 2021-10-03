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

        private static readonly ProjectionDefinition<BsonDocument> _entityIdProjection = Projection.Combine
        (
            Projection.Exclude(nameof(_id)),
            Projection.Include(nameof(EntityId))
        );

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
                        keys: IndexKeys.Combine
                        (
                            IndexKeys.Ascending(nameof(EntityId)),
                            IndexKeys.Ascending(nameof(EntityVersionNumber)),
                            IndexKeys.Ascending(nameof(EntitySubversionNumber))
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
            IEnumerable<FactDocument> factDocuments
        )
        {
            await InsertMany
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                factDocuments
            );
        }

        public static async Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            var factDocuments = await GetMany<FactDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                factQuery.GetFilter(_factFilterBuilder),
                factQuery.GetSort(_factSortBuilder),
                TransactionIdProjection
            );

            var transactionIds = factDocuments
                .Select(factDocument => factDocument.TransactionId)
                .Distinct();

            if (factQuery.Skip.HasValue)
            {
                transactionIds = transactionIds.Skip(factQuery.Skip.Value);
            }

            if (factQuery.Take.HasValue)
            {
                transactionIds = transactionIds.Take(factQuery.Take.Value);
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
            var factDocuments = await GetMany<FactDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                factQuery.GetFilter(_factFilterBuilder),
                factQuery.GetSort(_factSortBuilder),
                _entityIdProjection
            );

            var entityIds = factDocuments
                .Select(factDocument => factDocument.EntityId)
                .Distinct();

            if (factQuery.Skip.HasValue)
            {
                entityIds = entityIds.Skip(factQuery.Skip.Value);
            }

            if (factQuery.Take.HasValue)
            {
                entityIds = entityIds.Take(factQuery.Take.Value);
            }

            return entityIds.ToArray();
        }

        public static Task<List<FactDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            return GetMany<FactDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                factQuery.GetFilter(_factFilterBuilder),
                factQuery.GetSort(_factSortBuilder),
                DataProjection,
                skip: factQuery.Skip,
                limit: factQuery.Take
            );
        }
    }
}
