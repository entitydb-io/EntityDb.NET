using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Extensions
{
    /// <summary>
    ///     Extensions for the mongo client.
    /// </summary>
    public static class IMongoClientExtensions
    {
        private static readonly IndexKeysDefinitionBuilder<BsonDocument> IndexKeys = Builders<BsonDocument>.IndexKeys;

        private static readonly CreateIndexOptions UniquenessConstraint = new()
        {
            Name = "Uniqueness Constraint",
            Unique = true
        };

        private static readonly Dictionary<string, CreateIndexModel<BsonDocument>[]> _collections = new()
        {
            [SourceDocument.CollectionName] = new[]
            {
                new CreateIndexModel<BsonDocument>
                (
                    IndexKeys.Combine
                    (
                        IndexKeys.Descending(nameof(SourceDocument.TransactionId))
                    ),
                    UniquenessConstraint
                )
            },
            [CommandDocument.CollectionName] = new[]
            {
                new CreateIndexModel<BsonDocument>
                (
                    IndexKeys.Combine
                    (
                        IndexKeys.Descending(nameof(CommandDocument.EntityId)),
                        IndexKeys.Descending(nameof(CommandDocument.EntityVersionNumber))
                    ),
                    UniquenessConstraint
                )
            },
            [LeaseDocument.CollectionName] = new[]
            {
                new CreateIndexModel<BsonDocument>
                (
                    IndexKeys.Combine
                    (
                        IndexKeys.Descending(nameof(LeaseDocument.Scope)),
                        IndexKeys.Descending(nameof(LeaseDocument.Label)),
                        IndexKeys.Descending(nameof(LeaseDocument.Value))
                    ),
                    UniquenessConstraint
                )
            },
            [TagDocument.CollectionName] = new[]
            {
                new CreateIndexModel<BsonDocument>
                (
                    IndexKeys.Combine
                    (
                        IndexKeys.Descending(nameof(TagDocument.Label)),
                        IndexKeys.Descending(nameof(TagDocument.Value))
                    ),
                    new CreateIndexOptions { Name = "Lookup Index" }
                )
            }
        };

        /// <summary>
        ///     Provisions the needed collections on the database.
        /// </summary>
        /// <param name="mongoClient">The mongo client.</param>
        /// <param name="entityName">The name of the entity, which is used as the database name.</param>
        /// <returns>An asynchronous task that, when complete, signals that the collections have been provisioned.</returns>
        /// <remarks>
        ///     You should ONLY use this in your code for integration testing. Real databases should be provisioned using the
        ///     dotnet tool EntityDb.MongoDb.Provisioner.
        /// </remarks>
        public static async Task ProvisionCollections(this IMongoClient mongoClient, string entityName)
        {
            var mongoDatabase = mongoClient.GetDatabase(entityName);

            foreach (var (collectionName, collectionIndices) in _collections)
            {
                var mongoCollection = DocumentBase.GetMongoCollection(mongoDatabase, collectionName);

                var entityCollectionNameCursor = await mongoDatabase.ListCollectionNamesAsync();
                var entityCollectionNames = await entityCollectionNameCursor.ToListAsync();

                if (!entityCollectionNames.Contains(collectionName))
                {
                    await mongoDatabase.CreateCollectionAsync(collectionName);

                    await mongoCollection.Indexes.CreateManyAsync(collectionIndices);
                }
            }
        }
    }
}
