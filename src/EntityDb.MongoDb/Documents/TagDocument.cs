using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Queries;
using EntityDb.Common.Queries.Filtered;
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
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery
        )
        {
            var query = new TransactionIdQuery<TagDocument>
            (
                tagQuery.GetFilter(_tagFilterBuilder),
                tagQuery.GetSort(_tagSortBuilder),
                tagQuery.Skip,
                tagQuery.Take
            );

            return query.DistinctGuids
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
            );
        }

        public static Task<Guid[]> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery
        )
        {
            var query = new EntityIdQuery<TagDocument>
            (
                tagQuery.GetFilter(_tagFilterBuilder),
                tagQuery.GetSort(_tagSortBuilder),
                tagQuery.Skip,
                tagQuery.Take
            );

            return query.DistinctGuids
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
            );
        }

        public static Task<List<TagDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery
        )
        {
            var query = new DataQuery<TagDocument>
            (
                tagQuery.GetFilter(_tagFilterBuilder),
                tagQuery.GetSort(_tagSortBuilder),
                tagQuery.Skip,
                tagQuery.Take
            );

            return query.Execute
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
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
