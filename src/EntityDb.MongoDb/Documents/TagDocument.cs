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
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal sealed record TagDocument : DocumentBase, IEntityDocument
    {
        public Guid EntityId { get; init; }
        public ulong EntityVersionNumber { get; init; }
        public string Label { get; init; } = default!;
        public string Value { get; init; } = default!;

        private static readonly TagFilterBuilder _tagFilterBuilder = new();
        private static readonly TagSortBuilder _tagSortBuilder = new();

        public const string CollectionName = "Tags";

        public static readonly string[] HoistedFieldNames = new[]
        {
            nameof(Label),
            nameof(Value),
        };

        public static IReadOnlyCollection<TagDocument> BuildMany<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return transactionCommand.Tags.Insert
                .Select(insertTag => new TagDocument
                {
                    TransactionTimeStamp = transaction.TimeStamp,
                    TransactionId = transaction.Id,
                    EntityId = transactionCommand.EntityId,
                    EntityVersionNumber = transactionCommand.ExpectedPreviousVersionNumber + 1,
                    Label = insertTag.Label,
                    Value = insertTag.Value,
                    Data = BsonDocumentEnvelope.Deconstruct(insertTag, logger)
                })
                .ToArray();
        }

        public static async Task InsertMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            IReadOnlyCollection<TagDocument> tagDocuments
        )
        {
            await InsertMany
            (
                clientSessionHandle,
                GetMongoCollection(mongoDatabase, CollectionName),
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
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
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
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
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
                    MongoCollection = GetMongoCollection(mongoDatabase, CollectionName),
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
                GetMongoCollection(mongoDatabase, CollectionName),
                deleteTagsQuery.GetFilter(_tagFilterBuilder)
            );
        }
    }
}
