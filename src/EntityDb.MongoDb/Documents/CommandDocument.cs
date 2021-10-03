using EntityDb.Abstractions.Queries;
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
    internal sealed record CommandDocument
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        Guid EntityId,
        ulong EntityVersionNumber,
        BsonDocumentEnvelope Data,
        ObjectId? _id = null
    ) : EntityDocumentBase
    (
        TransactionTimeStamp,
        TransactionId,
        EntityId,
        EntityVersionNumber,
        Data,
        _id
    )
    {
        private static readonly CommandFilterBuilder _commandFilterBuilder = new();
        private static readonly CommandSortBuilder _commandSortBuilder = new();

        private static readonly ProjectionDefinition<BsonDocument> _entityVersionNumberProjection = Projection.Combine
        (
            Projection.Exclude(nameof(_id)),
            Projection.Include(nameof(EntityVersionNumber))
        );

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
            return GetTransactionIds<CommandDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                commandQuery.GetFilter(_commandFilterBuilder),
                commandQuery.GetSort(_commandSortBuilder),
                commandQuery.Skip,
                commandQuery.Take
            );
        }

        public static Task<Guid[]> GetEntityIds
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ICommandQuery commandQuery
        )
        {
            return GetEntityIds<CommandDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                commandQuery.GetFilter(_commandFilterBuilder),
                commandQuery.GetSort(_commandSortBuilder),
                commandQuery.Skip,
                commandQuery.Take
            );
        }

        public static Task<List<CommandDocument>> GetMany
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ICommandQuery commandQuery
        )
        {
            return GetMany<CommandDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                commandQuery.GetFilter(_commandFilterBuilder),
                commandQuery.GetSort(_commandSortBuilder),
                DataProjection,
                commandQuery.Skip,
                commandQuery.Take
            );
        }

        public static async Task<ulong> GetPreviousVersionNumber
        (
            IClientSessionHandle clientSessionHandle,
            IMongoDatabase mongoDatabase,
            Guid entityId
        )
        {
            var getLastCommandQuery = new GetLastCommandQuery(entityId);

            var commandDocuments = await GetMany<CommandDocument>
            (
                clientSessionHandle,
                GetCollection(mongoDatabase),
                getLastCommandQuery.GetFilter(_commandFilterBuilder),
                getLastCommandQuery.GetSort(_commandSortBuilder),
                _entityVersionNumberProjection,
                getLastCommandQuery.Skip,
                getLastCommandQuery.Take
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
