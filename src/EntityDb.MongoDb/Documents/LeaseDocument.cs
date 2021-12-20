using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal sealed record LeaseDocument : DocumentBase, IEntityDocument
    {
        public const string CollectionName = "Leases";

        private static readonly LeaseFilterBuilder _filterBuilder = new();

        private static readonly LeaseSortBuilder _sortBuilder = new();

        public static readonly string[] HoistedFieldNames = { nameof(Scope), nameof(Label), nameof(Value) };

        public string Scope { get; init; } = default!;
        public string Label { get; init; } = default!;
        public string Value { get; init; } = default!;
        public Guid EntityId { get; init; }
        public ulong EntityVersionNumber { get; init; }

        public static IReadOnlyCollection<LeaseDocument> BuildMany<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionStep<TEntity> transactionStep
        )
        {
            return transactionStep.Leases.Insert
                .Select(insertLease => new LeaseDocument
                {
                    TransactionTimeStamp = transaction.TimeStamp,
                    TransactionId = transaction.Id,
                    EntityId = transactionStep.EntityId,
                    EntityVersionNumber = transactionStep.NextEntityVersionNumber,
                    Scope = insertLease.Scope,
                    Label = insertLease.Label,
                    Value = insertLease.Value,
                    Data = BsonDocumentEnvelope.Deconstruct(insertLease, logger)
                })
                .ToArray();
        }

        public static async Task InsertMany<TEntity>
        (
            IMongoSession mongoSession,
            IMongoDatabase mongoDatabase,
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionStep<TEntity> transactionStep
        )
        {
            await InsertMany
            (
                mongoSession,
                GetMongoCollection(mongoDatabase, CollectionName),
                BuildMany(logger, transaction, transactionStep)
            );
        }

        public static DocumentQuery<LeaseDocument> GetDocumentQuery
        (
            IMongoSession? mongoSession,
            IMongoDatabase mongoDatabase,
            ILeaseQuery leaseQuery
        )
        {
            return new DocumentQuery<LeaseDocument>
            (
                mongoSession,
                GetMongoCollection(mongoDatabase, CollectionName),
                leaseQuery.GetFilter(_filterBuilder),
                leaseQuery.GetSort(_sortBuilder),
                leaseQuery.Skip,
                leaseQuery.Take
            );
        }

        public static async Task DeleteMany
        (
            IMongoSession mongoSession,
            IMongoDatabase mongoDatabase,
            Guid entityId,
            IReadOnlyCollection<ILease> deleteLeases
        )
        {
            if (deleteLeases.Count == 0)
            {
                return;
            }

            var deleteLeasesQuery = new DeleteLeasesQuery(entityId, deleteLeases);

            await DeleteMany
            (
                mongoSession,
                GetMongoCollection(mongoDatabase, CollectionName),
                deleteLeasesQuery.GetFilter(_filterBuilder)
            );
        }
    }
}
