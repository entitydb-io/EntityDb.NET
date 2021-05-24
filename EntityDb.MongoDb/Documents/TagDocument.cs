using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
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
    internal sealed record TagDocument
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
        private static readonly TagFilterBuilder _tagFilterBuilder = new();
        private static readonly TagSortBuilder _tagSortBuilder = new();
        private static readonly IndexKeysDefinitionBuilder<BsonDocument> _indexKeys = Builders<BsonDocument>.IndexKeys;
        private static readonly ProjectionDefinitionBuilder<BsonDocument> _projection = Builders<BsonDocument>.Projection;

        public const string CollectionName = "Tags";

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
            ITag[] tags
        )
        {
            if (tags.Length > 0)
            {
                var mongoCollection = GetCollection(mongoDatabase);

                var tagDocuments = tags.Select(tag => new TagDocument
                (
                    transactionTimeStamp,
                    transactionId,
                    entityId,
                    entityVersionNumber,
                    tag.Scope,
                    tag.Label,
                    tag.Value,
                    BsonDocumentEnvelope.Deconstruct(tag, serviceProvider)
                ));

                await InsertMany
                (
                    clientSessionHandle,
                    mongoCollection,
                    tagDocuments
                );
            }
        }

        public static Task<List<TagDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery,
            ProjectionDefinition<BsonDocument, TagDocument> projection
        )
        {
            var mongoCollection = GetCollection(mongoDatabase);

            var filter = tagQuery.GetFilter(_tagFilterBuilder);
            var sort = tagQuery.GetSort(_tagSortBuilder);
            var skip = tagQuery.Skip;
            var take = tagQuery.Take;

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
            ITagQuery tagQuery
        )
        {
            var tagDocuments = await GetMany
            (
                clientSessionHandle,
                mongoDatabase,
                tagQuery,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(TransactionId))
                )
            );

            return tagDocuments
                .Select(tagDocument => tagDocument.TransactionId)
                .Distinct()
                .ToArray();
        }

        public static async Task<Guid[]> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery
        )
        {
            var tagDocuments = await GetMany
            (
                clientSessionHandle,
                mongoDatabase,
                tagQuery,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(EntityId))
                )
            );

            return tagDocuments
                .Select(tagDocument => tagDocument.EntityId)
                .Distinct()
                .ToArray();
        }

        public static async Task<ITag[]> GetTags
        (
            IServiceProvider serviceProvider,
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery
        )
        {
            var tagDocuments = await GetMany
            (
                clientSessionHandle,
                mongoDatabase,
                tagQuery,
                _projection.Combine
                (
                    _projection.Exclude(nameof(_id)),
                    _projection.Include(nameof(Data))
                )
            );

            return tagDocuments
                .Select(tagDocument => tagDocument.Data.Reconstruct<ITag>(serviceProvider))
                .ToArray();
        }

        public static async Task DeleteMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            Guid entityId,
            ITag[] tags
        )
        {
            if (tags.Length > 0)
            {
                var mongoCollection = GetCollection(mongoDatabase);

                var deleteTagsQuery = new DeleteTagsQuery(entityId, tags);

                var tagDocumentFilter = deleteTagsQuery.GetFilter(_tagFilterBuilder);

                await DeleteMany
                (
                    clientSessionHandle,
                    mongoCollection,
                    tagDocumentFilter
                );
            }
        }
    }
}
