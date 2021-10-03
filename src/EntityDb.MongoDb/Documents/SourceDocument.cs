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
                GetCollection(mongoDatabase),
                sourceDocument
            );
        }

        public static GuidQuery<SourceDocument> GetTransactionIdsQuery
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ISourceQuery sourceQuery
        )
        {
            return new TransactionIdQuery<SourceDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                sourceQuery.GetFilter(_sourceFilterBuilder),
                sourceQuery.GetSort(_sourceSortBuilder),
                sourceQuery.Skip,
                sourceQuery.Take
            );
        }

        public static GuidQuery<SourceDocument> GetEntityIdsQuery
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ISourceQuery sourceQuery
        )
        {
            return new EntityIdsQuery<SourceDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                sourceQuery.GetFilter(_sourceFilterBuilder),
                sourceQuery.GetSort(_sourceSortBuilder),
                sourceQuery.Skip,
                sourceQuery.Take
            );
        }

        public static DataQuery<SourceDocument> GetDataQuery
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ISourceQuery sourceQuery
        )
        {
            return new DataQuery<SourceDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                sourceQuery.GetFilter(_sourceFilterBuilder),
                sourceQuery.GetSort(_sourceSortBuilder),
                sourceQuery.Skip,
                sourceQuery.Take
            );
        }
    }
}
