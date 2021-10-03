using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
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

        public static IEnumerable<FactDocument> BuildMany<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return transactionCommand.Facts
                .Select(transactionFact => new FactDocument
                (
                    transaction.TimeStamp,
                    transaction.Id,
                    transactionCommand.EntityId,
                    transactionCommand.ExpectedPreviousVersionNumber + 1,
                    transactionFact.SubversionNumber,
                    BsonDocumentEnvelope.Deconstruct(transactionFact.Fact, logger)
                ));
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

        public static TransactionIdQuery<FactDocument> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            return new TransactionIdQuery<FactDocument>
            {
                ClientSessionHandle = clientSessionHandle,
                MongoCollection = GetCollection(mongoDatabase),
                Filter = factQuery.GetFilter(_factFilterBuilder),
                Sort = factQuery.GetSort(_factSortBuilder),
                DistinctSkip = factQuery.Skip,
                DistinctLimit = factQuery.Take
            };
        }

        public static EntityIdQuery<FactDocument> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            return new EntityIdQuery<FactDocument>
            {
                ClientSessionHandle = clientSessionHandle,
                MongoCollection = GetCollection(mongoDatabase),
                Filter = factQuery.GetFilter(_factFilterBuilder),
                Sort = factQuery.GetSort(_factSortBuilder),
                DistinctSkip = factQuery.Skip,
                DistinctLimit = factQuery.Take
            };
        }

        public static DataQuery<FactDocument> GetData
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IFactQuery factQuery
        )
        {
            return new DataQuery<FactDocument>
            {
                ClientSessionHandle = clientSessionHandle,
                MongoCollection = GetCollection(mongoDatabase),
                Filter = factQuery.GetFilter(_factFilterBuilder),
                Sort = factQuery.GetSort(_factSortBuilder),
                Skip = factQuery.Skip,
                Limit = factQuery.Take
            };
        }
    }
}
