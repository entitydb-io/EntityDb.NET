using EntityDb.Abstractions.Queries;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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
    ), IEntityDocument
    {
        private static readonly FactFilterBuilder _factFilterBuilder = new();
        private static readonly FactSortBuilder _factSortBuilder = new();

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

        public static Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            var query = new TransactionIdQuery<FactDocument>
            (
                factQuery.GetFilter(_factFilterBuilder),
                factQuery.GetSort(_factSortBuilder),
                factQuery.Skip,
                factQuery.Take
            );

            return query.DistinctGuids
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
            );
        }

        public static Task<Guid[]> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            var query = new EntityIdQuery<FactDocument>
            (
                factQuery.GetFilter(_factFilterBuilder),
                factQuery.GetSort(_factSortBuilder),
                factQuery.Skip,
                factQuery.Take
            );

            return query.DistinctGuids
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
            );
        }

        public static Task<List<FactDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            var query = new DataQuery<FactDocument>
            (
                factQuery.GetFilter(_factFilterBuilder),
                factQuery.GetSort(_factSortBuilder),
                factQuery.Skip,
                factQuery.Take
            );

            return query.Execute
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
            );
        }
    }
}
