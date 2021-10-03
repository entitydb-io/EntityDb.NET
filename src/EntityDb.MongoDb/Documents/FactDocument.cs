using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
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
    internal sealed record FactDocument : DocumentBase, IEntityDocument
    {
        public Guid EntityId { get; init; }
        public ulong EntityVersionNumber { get; init; }
        public ulong EntitySubversionNumber { get; init; }

        private static readonly FactFilterBuilder _factFilterBuilder = new();
        private static readonly FactSortBuilder _factSortBuilder = new();

        public const string CollectionName = "Facts";

        public static IReadOnlyCollection<FactDocument> BuildMany<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return transactionCommand.Facts
                .Select(transactionFact => new FactDocument
                {
                    TransactionTimeStamp = transaction.TimeStamp,
                    TransactionId = transaction.Id,
                    EntityId = transactionCommand.EntityId,
                    EntityVersionNumber = transactionCommand.ExpectedPreviousVersionNumber + 1,
                    EntitySubversionNumber = transactionFact.SubversionNumber,
                    Data = BsonDocumentEnvelope.Deconstruct(transactionFact.Fact, logger)
                })
                .ToArray();
        }

        public static async Task InsertMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IReadOnlyCollection<FactDocument> factDocuments
        )
        {
            await InsertMany
            (
                clientSessionHandle,
                GetMongoCollection(mongoDatabase, CollectionName),
                factDocuments
            );
        }

        public static Task<Guid[]> GetTransactionIds
        (
            IMongoDbSession mongoDbSession,
            IFactQuery factQuery
        )
        {
            return mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => new TransactionIdQuery<FactDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = factQuery.GetFilter(_factFilterBuilder),
                    Sort = factQuery.GetSort(_factSortBuilder),
                    DistinctSkip = factQuery.Skip,
                    DistinctLimit = factQuery.Take
                }
            );
        }

        public static Task<Guid[]> GetEntityIds
        (
            IMongoDbSession mongoDbSession,
            IFactQuery factQuery
        )
        {
            return mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => new EntityIdQuery<FactDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = factQuery.GetFilter(_factFilterBuilder),
                    Sort = factQuery.GetSort(_factSortBuilder),
                    DistinctSkip = factQuery.Skip,
                    DistinctLimit = factQuery.Take
                }
            );
        }

        public static Task<IFact<TEntity>[]> GetData<TEntity>
        (
            IMongoDbSession mongoDbSession,
            IFactQuery factQuery
        )
        {
            return mongoDbSession.ExecuteDataQuery<FactDocument, IFact<TEntity>>
            (
                (clientSessionHandle, mongoDatabase) => new DataQuery<FactDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = factQuery.GetFilter(_factFilterBuilder),
                    Sort = factQuery.GetSort(_factSortBuilder),
                    Skip = factQuery.Skip,
                    Limit = factQuery.Take
                }
            );
        }
    }
}
