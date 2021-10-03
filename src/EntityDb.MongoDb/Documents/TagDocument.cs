using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Queries;
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
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal sealed record TagDocument
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        Guid EntityId,
        ulong EntityVersionNumber,
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
        private static readonly TagFilterBuilder _tagFilterBuilder = new();
        private static readonly TagSortBuilder _tagSortBuilder = new();

        public const string CollectionName = "Tags";

        public static readonly string[] HoistedFieldNames = new[]
        {
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
                Array.Empty<CreateIndexModel<BsonDocument>>()
            );
        }

        public static IEnumerable<TagDocument> BuildMany<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return transactionCommand.InsertTags
                .Select(insertTag => new TagDocument
                (
                    transaction.TimeStamp,
                    transaction.Id,
                    transactionCommand.EntityId,
                    transactionCommand.ExpectedPreviousVersionNumber + 1,
                    insertTag.Label,
                    insertTag.Value,
                    BsonDocumentEnvelope.Deconstruct(insertTag, logger)
                ));
        }

        public static async Task InsertMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IEnumerable<TagDocument> tagDocuments
        )
        {
            await InsertMany
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                tagDocuments
            );
        }

        public static Task<Guid[]> GetTransactionIds
        (
            IMongoDbSession mongoDbSession,
            ITagQuery tagQuery
        )
        {
            return mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => new TransactionIdQuery<TagDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetCollection(mongoDatabase),
                    Filter = tagQuery.GetFilter(_tagFilterBuilder),
                    Sort = tagQuery.GetSort(_tagSortBuilder),
                    DistinctSkip = tagQuery.Skip,
                    DistinctLimit = tagQuery.Take
                }
            );
        }

        public static Task<Guid[]> GetEntityIds
        (
            IMongoDbSession mongoDbSession,
            ITagQuery tagQuery
        )
        {
            return mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => new EntityIdQuery<TagDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetCollection(mongoDatabase),
                    Filter = tagQuery.GetFilter(_tagFilterBuilder),
                    Sort = tagQuery.GetSort(_tagSortBuilder),
                    DistinctSkip = tagQuery.Skip,
                    DistinctLimit = tagQuery.Take
                }
            );
        }

        public static Task<ITag[]> GetData
        (
            IMongoDbSession mongoDbSession,
            ITagQuery tagQuery
        )
        {
            return mongoDbSession.ExecuteDataQuery<TagDocument, ITag>
            (
                (clientSessionHandle, mongoDatabase) => new DataQuery<TagDocument>
                {
                    ClientSessionHandle = clientSessionHandle,
                    MongoCollection = GetCollection(mongoDatabase),
                    Filter = tagQuery.GetFilter(_tagFilterBuilder),
                    Sort = tagQuery.GetSort(_tagSortBuilder),
                    Skip = tagQuery.Skip,
                    Limit = tagQuery.Take
                }
            );
        }

        public static async Task DeleteMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            Guid entityId,
            IReadOnlyCollection<ITag> deleteTags
        )
        {
            if (deleteTags.Count == 0)
            {
                return;
            }

            var deleteTagsQuery = new DeleteTagsQuery(entityId, deleteTags);

            await DeleteMany
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                deleteTagsQuery.GetFilter(_tagFilterBuilder)
            );
        }
    }
}
