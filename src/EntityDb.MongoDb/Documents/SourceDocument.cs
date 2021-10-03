using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
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
    ) : DocumentBase
    (
        TransactionTimeStamp,
        TransactionId,
        Data,
        _id
    ), IEntitiesDocument
    {
        private static readonly SourceFilterBuilder _sourceFilterBuilder = new();
        private static readonly SourceSortBuilder _sourceSortBuilder = new();

        public const string CollectionName = "Sources";

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

        public static SourceDocument BuildOne<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction
        )
        {
            return new SourceDocument
            (
                transaction.TimeStamp,
                transaction.Id,
                transaction.Commands.Select(command => command.EntityId).Distinct().ToArray(),
                BsonDocumentEnvelope.Deconstruct(transaction.Source, logger)
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
                GetMongoCollection(mongoDatabase, CollectionName),
                sourceDocument
            );
        }

        public static Task<Guid[]> GetTransactionIds
        (
            IMongoDbSession mongoDbSession,
            ISourceQuery sourceQuery
        )
        {
            return mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => new TransactionIdQuery<SourceDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = sourceQuery.GetFilter(_sourceFilterBuilder),
                    Sort = sourceQuery.GetSort(_sourceSortBuilder),
                    DistinctSkip = sourceQuery.Skip,
                    DistinctLimit = sourceQuery.Take
                }
            );
        }

        public static Task<Guid[]> GetEntityIds
        (
            IMongoDbSession mongoDbSession,
            ISourceQuery sourceQuery
        )
        {
            return mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => new EntityIdsQuery<SourceDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = sourceQuery.GetFilter(_sourceFilterBuilder),
                    Sort = sourceQuery.GetSort(_sourceSortBuilder),
                    DistinctSkip = sourceQuery.Skip,
                    DistinctLimit = sourceQuery.Take
                }
            );
        }

        public static Task<object[]> GetData
        (
            IMongoDbSession mongoDbSession,
            ISourceQuery sourceQuery
        )
        {
            return mongoDbSession.ExecuteDataQuery<SourceDocument, object>
            (
                (clientSessionHandle, mongoDatabase) => new DataQuery<SourceDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
                    Filter = sourceQuery.GetFilter(_sourceFilterBuilder),
                    Sort = sourceQuery.GetSort(_sourceSortBuilder),
                    Skip = sourceQuery.Skip,
                    Limit = sourceQuery.Take
                }
            );
        }
    }
}
