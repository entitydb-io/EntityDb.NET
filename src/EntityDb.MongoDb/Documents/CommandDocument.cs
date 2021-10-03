using EntityDb.Abstractions.Queries;
using EntityDb.Common.Queries;
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
    internal sealed record CommandDocument
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        Guid EntityId,
        ulong EntityVersionNumber,
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
        private static readonly CommandFilterBuilder _commandFilterBuilder = new();
        private static readonly CommandSortBuilder _commandSortBuilder = new();

        public const string CollectionName = "Commands";

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
                            IndexKeys.Descending(nameof(EntityId)),
                            IndexKeys.Descending(nameof(EntityVersionNumber))
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
            CommandDocument commandDocument
        )
        {
            return InsertOne
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                commandDocument
            );
        }

        public static Task<Guid[]> GetTransactionIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ICommandQuery commandQuery
        )
        {
            var query = new TransactionIdQuery<CommandDocument>
            (
                commandQuery.GetFilter(_commandFilterBuilder),
                commandQuery.GetSort(_commandSortBuilder),
                commandQuery.Skip,
                commandQuery.Take
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
            ICommandQuery commandQuery
        )
        {
            var query = new EntityIdQuery<CommandDocument>
            (
                commandQuery.GetFilter(_commandFilterBuilder),
                commandQuery.GetSort(_commandSortBuilder),
                commandQuery.Skip,
                commandQuery.Take
            );

            return query.DistinctGuids
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
            );
        }

        public static Task<List<CommandDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ICommandQuery commandQuery
        )
        {
            var query = new DataQuery<CommandDocument>
            (
                commandQuery.GetFilter(_commandFilterBuilder),
                commandQuery.GetSort(_commandSortBuilder),
                commandQuery.Skip,
                commandQuery.Take
            );

            return query.Execute
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
            );
        }

        public static async Task<ulong> GetLastEntityVersionNumber
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            Guid entityId
        )
        {
            var commandQuery = new GetLastEntityVersionQuery(entityId);

            var query = new EntityVersionQuery<CommandDocument>
            (
                commandQuery.GetFilter(_commandFilterBuilder),
                commandQuery.GetSort(_commandSortBuilder),
                commandQuery.Skip,
                commandQuery.Take
            );

            var commandDocuments = await query.Execute
            (
                clientSessionHandle,
                GetCollection(mongoDatabase)
            );

            var lastCommandDocument = commandDocuments.SingleOrDefault();

            if (lastCommandDocument == null)
            {
                return default;
            }

            return lastCommandDocument.EntityVersionNumber;
        }
    }
}
