using EntityDb.Abstractions.Queries;
using EntityDb.Common.Queries.Filtered;
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

        private static readonly ProjectionDefinition<BsonDocument> _entityIdProjection = Projection.Combine
        (
            Projection.Exclude(nameof(_id)),
            Projection.Include(nameof(EntityId))
        );

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

        public static async Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery
        )
        {
            var tagDocuments = await GetMany<TagDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                tagQuery.GetFilter(_tagFilterBuilder),
                tagQuery.GetSort(_tagSortBuilder),
                TransactionIdProjection
            );

            var transactionIds = tagDocuments
                .Select(tagDocument => tagDocument.TransactionId)
                .Distinct();

            if (tagQuery.Skip.HasValue)
            {
                transactionIds = transactionIds.Skip(tagQuery.Skip.Value);
            }

            if (tagQuery.Take.HasValue)
            {
                transactionIds = transactionIds.Take(tagQuery.Take.Value);
            }

            return transactionIds.ToArray();
        }

        public static async Task<Guid[]> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery
        )
        {
            var tagDocuments = await GetMany<TagDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                tagQuery.GetFilter(_tagFilterBuilder),
                tagQuery.GetSort(_tagSortBuilder),
                _entityIdProjection
            );

            var entityIds = tagDocuments
                .Select(tagDocument => tagDocument.EntityId)
                .Distinct();

            if (tagQuery.Skip.HasValue)
            {
                entityIds = entityIds.Skip(tagQuery.Skip.Value);
            }

            if (tagQuery.Take.HasValue)
            {
                entityIds = entityIds.Take(tagQuery.Take.Value);
            }

            return entityIds.ToArray();
        }

        public static Task<List<TagDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery
        )
        {
            return GetMany<TagDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                tagQuery.GetFilter(_tagFilterBuilder),
                tagQuery.GetSort(_tagSortBuilder),
                DataProjection,
                tagQuery.Skip,
                tagQuery.Take
            );
        }

        public static async Task DeleteMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagFilter tagFilter
        )
        {
            await DeleteMany
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                tagFilter.GetFilter(_tagFilterBuilder)
            );
        }
    }
}
