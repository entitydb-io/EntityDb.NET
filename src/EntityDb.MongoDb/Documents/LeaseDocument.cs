using EntityDb.Abstractions.Queries;
using EntityDb.Common.Queries.Filtered;
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
    internal sealed record LeaseDocument
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        Guid EntityId,
        ulong EntityVersionNumber,
        string Scope,
        string Label,
        string Value,
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
        private static readonly LeaseFilterBuilder _leaseFilterBuilder = new();
        private static readonly LeaseSortBuilder _leaseSortBuilder = new();

        private static readonly ProjectionDefinition<BsonDocument> _entityIdProjection = Projection.Combine
        (
            Projection.Exclude(nameof(_id)),
            Projection.Include(nameof(EntityId))
        );

        public const string CollectionName = "Leases";

        public static readonly string[] HoistedFieldNames = new[]
        {
            nameof(Scope),
            nameof(Label),
            nameof(Value),
        };

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
                            IndexKeys.Descending(nameof(Scope)),
                            IndexKeys.Descending(nameof(Label)),
                            IndexKeys.Descending(nameof(Value))
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
            IEnumerable<LeaseDocument> leaseDocuments
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);


            await InsertMany
            (
                clientSessionHandle,
                mongoCollection,
                leaseDocuments
            );
        }

        public static async Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery
        )
        {
            var leaseDocuments = await GetMany<LeaseDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                leaseQuery.GetFilter(_leaseFilterBuilder),
                leaseQuery.GetSort(_leaseSortBuilder),
                TransactionIdProjection
            );

            var transactionIds = leaseDocuments
                .Select(leaseDocument => leaseDocument.TransactionId)
                .Distinct();

            if (leaseQuery.Skip.HasValue)
            {
                transactionIds = transactionIds.Skip(leaseQuery.Skip.Value);
            }

            if (leaseQuery.Take.HasValue)
            {
                transactionIds = transactionIds.Take(leaseQuery.Take.Value);
            }

            return transactionIds.ToArray();
        }

        public static async Task<Guid[]> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery
        )
        {
            var leaseDocuments = await GetMany<LeaseDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                leaseQuery.GetFilter(_leaseFilterBuilder),
                leaseQuery.GetSort(_leaseSortBuilder),
                _entityIdProjection
            );

            var entityIds = leaseDocuments
                .Select(leaseDocument => leaseDocument.EntityId)
                .Distinct();

            if (leaseQuery.Skip.HasValue)
            {
                entityIds = entityIds.Skip(leaseQuery.Skip.Value);
            }

            if (leaseQuery.Take.HasValue)
            {
                entityIds = entityIds.Take(leaseQuery.Take.Value);
            }

            return entityIds.ToArray();
        }

        public static Task<List<LeaseDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery
        )
        {
            return GetMany<LeaseDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                leaseQuery.GetFilter(_leaseFilterBuilder),
                leaseQuery.GetSort(_leaseSortBuilder),
                DataProjection,
                leaseQuery.Skip,
                leaseQuery.Take
            );
        }

        public static async Task DeleteMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseFilter leaseFilter
        )
        {
            await DeleteMany
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                leaseFilter.GetFilter(_leaseFilterBuilder)
            );
        }
    }
}
