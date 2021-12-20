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
        public const string CollectionName = "Tags";

        public static readonly TagFilterBuilder _filterBuilder = new();

        public static readonly TagSortBuilder _sortBuilder = new();

        public static readonly string[] HoistedFieldNames = { nameof(Label), nameof(Value) };

        public string Label { get; init; } = default!;
        public string Value { get; init; } = default!;
        public Guid EntityId { get; init; }
        public ulong EntityVersionNumber { get; init; }

        public static IReadOnlyCollection<TagDocument> BuildMany<TEntity>
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionStep<TEntity> transactionStep
        )
        {
            return transactionStep.Tags.Insert
                .Select(insertTag => new TagDocument
                {
                    TransactionTimeStamp = transaction.TimeStamp,
                    TransactionId = transaction.Id,
                    EntityId = transactionStep.EntityId,
                    EntityVersionNumber = transactionStep.NextEntityVersionNumber,
                    Label = insertTag.Label,
                    Value = insertTag.Value,
                    Data = BsonDocumentEnvelope.Deconstruct(insertTag, logger)
                })
                .ToArray();
        }

        public static async Task InsertMany<TEntity>
        (
            IMongoSession mongoSession,
            IMongoDatabase mongoDatabase,
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionStep<TEntity> transactionStep
        )
        {
            await InsertMany
            (
                mongoSession,
                GetMongoCollection(mongoDatabase, CollectionName),
                BuildMany(logger, transaction, transactionStep)
            );
        }

        public static DocumentQuery<TagDocument> GetDocumentQuery
        (
            IMongoSession? mongoSession,
            IMongoDatabase mongoDatabase,
            ITagQuery tagQuery
        )
        {
            return new DocumentQuery<TagDocument>
            (
                mongoSession,
                GetMongoCollection(mongoDatabase, CollectionName),
                tagQuery.GetFilter(_filterBuilder),
                tagQuery.GetSort(_sortBuilder),
                tagQuery.Skip,
                tagQuery.Take
            );
        }

        public static async Task DeleteMany
        (
            IMongoSession mongoSession,
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
                mongoSession,
                GetMongoCollection(mongoDatabase, CollectionName),
                deleteTagsQuery.GetFilter(_filterBuilder)
            );
        }
    }
}
