using EntityDb.Abstractions.Queries;
using EntityDb.Common.Queries.Filtered;
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
    ), IEntityDocument
    {
        private static readonly LeaseFilterBuilder _leaseFilterBuilder = new();
        private static readonly LeaseSortBuilder _leaseSortBuilder = new();

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
            await InsertMany
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                leaseDocuments
            );
        }

        public static Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery
        )
        {
            var query = new TransactionIdQuery<LeaseDocument>
            (
                leaseQuery.GetFilter(_leaseFilterBuilder),
                leaseQuery.GetSort(_leaseSortBuilder),
                leaseQuery.Skip,
                leaseQuery.Take
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
            ILeaseQuery leaseQuery
        )
        {
            var query = new EntityIdQuery<LeaseDocument>
            (
                leaseQuery.GetFilter(_leaseFilterBuilder),
                leaseQuery.GetSort(_leaseSortBuilder),
                leaseQuery.Skip,
                leaseQuery.Take
            );

            return query.DistinctGuids
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
            );
        }

        public static Task<List<LeaseDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery
        )
        {
            var query = new DataQuery<LeaseDocument>
            (
                leaseQuery.GetFilter(_leaseFilterBuilder),
                leaseQuery.GetSort(_leaseSortBuilder),
                leaseQuery.Skip,
                leaseQuery.Take
            );

            return query.Execute(clientSessionHandle, GetCollection(mongoDatabase));
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
