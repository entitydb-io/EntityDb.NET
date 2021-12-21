using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Commands;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IReadOnlyCollection<LeaseDocument>? BuildInsert<TEntity>
        (
            ITransaction<TEntity> transaction,
            int transactionStepIndex,
            ILogger logger
        )
        {
            var transactionStep = transaction.Steps[transactionStepIndex];
            var insertLeases = transactionStep.Leases.Insert;

            if (insertLeases.Length == 0)
            {
                return null;
            }

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

        public static FilterDefinition<BsonDocument>? BuildDelete<TEntity>
        (
            ITransaction<TEntity> transaction,
            int transactionStepIndex
        )
        {
            var transactionStep = transaction.Steps[transactionStepIndex];
            var deleteLeases = transactionStep.Leases.Delete;

            if (deleteLeases.Length == 0)
            {
                return null;
            }

            return new DeleteLeasesQuery(transactionStep.EntityId, deleteLeases)
                .GetFilter(_filterBuilder);
        }

        public static InsertDocumentsCommand<TEntity, LeaseDocument> GetInsertCommand<TEntity>
        (
            IMongoSession mongoSession
        )
        {
            return new InsertDocumentsCommand<TEntity, LeaseDocument>
            (
                mongoSession,
                CollectionName,
                BuildInsert<TEntity>
            );
        }

        public static DocumentQuery<LeaseDocument> GetQuery
        (
            IMongoSession mongoSession,
            ILeaseQuery leaseQuery
        )
        {
            return new DocumentQuery<LeaseDocument>
            (
                mongoSession,
                CollectionName,
                leaseQuery.GetFilter(_filterBuilder),
                leaseQuery.GetSort(_sortBuilder),
                leaseQuery.Skip,
                leaseQuery.Take
            );
        }

        public static DeleteDocumentsCommand<TEntity, LeaseDocument> GetDeleteCommand<TEntity>
        (
            IMongoSession mongoSession
        )
        {
            return new DeleteDocumentsCommand<TEntity, LeaseDocument>
            (
                mongoSession,
                CollectionName,
                BuildDelete<TEntity>
            );
        }
    }
}
