using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Extensions;

/// <summary>
///     Extensions for the mongo client.
/// </summary>
public static class MongoClientExtensions
{
    private static readonly IndexKeysDefinitionBuilder<BsonDocument>
        IndexKeysBuilder = Builders<BsonDocument>.IndexKeys;

    private static readonly CreateIndexOptions UniquenessConstraint = new()
    {
        Name = "Uniqueness Constraint",
        Unique = true
    };

    private static readonly Dictionary<string, CreateIndexModel<BsonDocument>[]> Collections = new()
    {
        [AgentSignatureDocument.CollectionName] = new[]
        {
            new CreateIndexModel<BsonDocument>
            (
                IndexKeysBuilder.Combine
                (
                    IndexKeysBuilder.Descending(nameof(AgentSignatureDocument.TransactionId))
                ),
                UniquenessConstraint
            )
        },
        [CommandDocument.CollectionName] = new[]
        {
            new CreateIndexModel<BsonDocument>
            (
                IndexKeysBuilder.Combine
                (
                    IndexKeysBuilder.Descending(nameof(CommandDocument.EntityId)),
                    IndexKeysBuilder.Descending(nameof(CommandDocument.EntityVersionNumber))
                ),
                UniquenessConstraint
            )
        },
        [LeaseDocument.CollectionName] = new[]
        {
            new CreateIndexModel<BsonDocument>
            (
                IndexKeysBuilder.Combine
                (
                    IndexKeysBuilder.Descending(nameof(LeaseDocument.Scope)),
                    IndexKeysBuilder.Descending(nameof(LeaseDocument.Label)),
                    IndexKeysBuilder.Descending(nameof(LeaseDocument.Value))
                ),
                UniquenessConstraint
            )
        },
        [TagDocument.CollectionName] = new[]
        {
            new CreateIndexModel<BsonDocument>
            (
                IndexKeysBuilder.Combine
                (
                    IndexKeysBuilder.Descending(nameof(TagDocument.Label)),
                    IndexKeysBuilder.Descending(nameof(TagDocument.Value))
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
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An asynchronous task that, when complete, signals that the collections have been provisioned.</returns>
    /// <remarks>
    ///     You should ONLY use this in your code for integration testing. Real databases should be provisioned using the
    ///     dotnet tool EntityDb.MongoDb.Provisioner.
    /// </remarks>
    public static async Task ProvisionCollections(this IMongoClient mongoClient, string entityName,
        CancellationToken cancellationToken = default)
    {
        var mongoDatabase = mongoClient.GetDatabase(entityName);

        foreach (var (collectionName, collectionIndices) in Collections)
        {
            var mongoCollection = mongoDatabase.GetCollection<BsonDocument>(collectionName);

            var entityCollectionNameCursor =
                await mongoDatabase.ListCollectionNamesAsync(cancellationToken: cancellationToken);
            var entityCollectionNames = await entityCollectionNameCursor.ToListAsync(cancellationToken);

            if (entityCollectionNames.Contains(collectionName))
            {
                continue;
            }

            await mongoDatabase.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);

            await mongoCollection.Indexes.CreateManyAsync(collectionIndices, cancellationToken);
        }
    }
}
