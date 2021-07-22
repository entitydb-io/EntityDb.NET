using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Leases;
using EntityDb.Common.Queries;
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
        private static readonly IndexKeysDefinitionBuilder<BsonDocument> _indexKeys = Builders<BsonDocument>.IndexKeys;
        private static readonly ProjectionDefinitionBuilder<BsonDocument> _projection = Builders<BsonDocument>.Projection;

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
                        keys: _indexKeys.Combine
                        (
                            _indexKeys.Descending(nameof(Scope)),
                            _indexKeys.Descending(nameof(Label)),
                            _indexKeys.Descending(nameof(Value))
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
            IServiceProvider serviceProvider,
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            DateTime transactionTimeStamp,
            Guid transactionId,
            Guid entityId,
            ulong entityVersionNumber,
            ILease[] leases
        )
        {
            if (leases.Length > 0)
            {
                var mongoCollection = GetCollection(mongoDatabase);

                var leaseDocuments = leases.Select(lease => new LeaseDocument
                (
                    transactionTimeStamp,
                    transactionId,
                    entityId,
                    entityVersionNumber,
                    lease.Scope,
                    lease.Label,
                    lease.Value,
                    BsonDocumentEnvelope.Deconstruct(lease, serviceProvider)
                ));

                await InsertMany
                (
                    clientSessionHandle,
                    mongoCollection,
                    leaseDocuments
                );
            }
        }

        public static Task<List<LeaseDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery,
            ProjectionDefinition<BsonDocument, LeaseDocument> projection
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = leaseQuery.GetFilter(_leaseFilterBuilder);
            var sort = leaseQuery.GetSort(_leaseSortBuilder);
            var skip = leaseQuery.Skip;
            var take = leaseQuery.Take;

            return GetMany
            (
                clientSessionHandle,
                mongoCollection,
                filter,
                sort,
                projection,
                skip,
                take
            );
        }

        public static async Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery
        )
        {
            var leaseDocuments = await GetMany
            (
                clientSessionHandle,
                mongoDatabase,
                leaseQuery,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(TransactionId))
                )
            );

            return leaseDocuments
                .Select(leaseDocument => leaseDocument.TransactionId)
                .Distinct()
                .ToArray();
        }

        public static async Task<Guid[]> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery
        )
        {
            var leaseDocuments = await GetMany
            (
                clientSessionHandle,
                mongoDatabase,
                leaseQuery,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(EntityId))
                )
            );

            return leaseDocuments
                .Select(leaseDocument => leaseDocument.EntityId)
                .Distinct()
                .ToArray();
        }

        public static async Task<ILease[]> GetLeases
        (
            IServiceProvider serviceProvider,
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery
        )
        {
            var leaseDocuments = await GetMany
            (
                clientSessionHandle,
                mongoDatabase,
                leaseQuery,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(Data))
                )
            );

            return leaseDocuments
                .Select(leaseDocument => leaseDocument.Data.Reconstruct<ILease>(serviceProvider))
                .ToArray();
        }

        public static async Task DeleteMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            Guid entityId,
            ILease[] leases
        )
        {
            if (leases.Length > 0)
            {
                var mongoCollection = GetCollection(mongoDatabase);

                var deleteLeasesQuery = new DeleteLeasesQuery(entityId, leases);

                var leaseDocumentFilter = deleteLeasesQuery.GetFilter(_leaseFilterBuilder);

                await DeleteMany
                (
                    clientSessionHandle,
                    mongoCollection,
                    leaseDocumentFilter
                );
            }
        }
    }
}
