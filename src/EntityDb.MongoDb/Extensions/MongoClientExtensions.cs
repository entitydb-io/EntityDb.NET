using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Extensions;

/// <summary>
///     Extensions for the mongo client.
/// </summary>
public static class MongoClientExtensions
{
    private static readonly IndexKeysDefinitionBuilder<BsonDocument>
        IndexKeysBuilder = Builders<BsonDocument>.IndexKeys;

    private static readonly CreateIndexOptions UniquenessConstraint = new()
    {
        Name = "Uniqueness Constraint", Unique = true,
    };

    private static readonly CreateIndexOptions LookupIndex = new() { Name = "Lookup Index" };

    private static readonly Dictionary<string, CreateIndexModel<BsonDocument>[]> SourceCollections = new()
    {
        [AgentSignatureDocument.CollectionName] = new[]
        {
            new CreateIndexModel<BsonDocument>
            (
                IndexKeysBuilder.Combine
                (
                    IndexKeysBuilder.Descending(nameof(AgentSignatureDocument.SourceId))
                ),
                UniquenessConstraint
            ),
        },
        [DeltaDocument.CollectionName] = new[]
        {
            new CreateIndexModel<BsonDocument>
            (
                IndexKeysBuilder.Combine
                (
                    IndexKeysBuilder.Descending(nameof(DeltaDocument.EntityId)),
                    IndexKeysBuilder.Descending(nameof(DeltaDocument.EntityVersion))
                ),
                UniquenessConstraint
            ),
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
            ),
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
                LookupIndex
            ),
        },
    };

    private static readonly CreateIndexModel<BsonDocument>[] SnapshotCollection =
    {
        new(
            IndexKeysBuilder.Combine
            (
                IndexKeysBuilder.Descending(nameof(SnapshotDocument.SnapshotId)),
                IndexKeysBuilder.Descending(nameof(SnapshotDocument.SnapshotVersion))
            ),
            UniquenessConstraint
        ),
    };

    /// <summary>
    ///     Provisions the needed collections on the database.
    /// </summary>
    /// <param name="mongoClient">The mongo client.</param>
    /// <param name="serviceName">The name of the service, which is used as the name of the database.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An asynchronous task that, when complete, signals that the collections have been provisioned.</returns>
    /// <remarks>
    ///     You should ONLY use this in your code for integration testing. Real databases should be provisioned using the
    ///     dotnet tool EntityDb.MongoDb.Provisioner.
    /// </remarks>
    public static async Task ProvisionSourceCollections(this IMongoClient mongoClient, string serviceName,
        CancellationToken cancellationToken = default)
    {
        var mongoDatabase = mongoClient.GetDatabase(serviceName);

        foreach (var (collectionName, collectionIndices) in SourceCollections)
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

    /// <summary>
    ///     Provisions the needed collections on the database.
    /// </summary>
    /// <param name="mongoClient">The mongo client.</param>
    /// <param name="serviceName">The name of the service, which is used as the name of the database.</param>
    /// <param name="collectionName">The name of the collection</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An asynchronous task that, when complete, signals that the collections have been provisioned.</returns>
    /// <remarks>
    ///     You should ONLY use this in your code for integration testing. Real databases should be provisioned using the
    ///     dotnet tool EntityDb.MongoDb.Provisioner.
    /// </remarks>
    public static async Task ProvisionSnapshotCollection(this IMongoClient mongoClient, string serviceName,
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var collectionIndices = SnapshotCollection;

        var mongoDatabase = mongoClient.GetDatabase(serviceName);

        var mongoCollection = mongoDatabase.GetCollection<BsonDocument>(collectionName);

        var entityCollectionNameCursor =
            await mongoDatabase.ListCollectionNamesAsync(cancellationToken: cancellationToken);
        var entityCollectionNames = await entityCollectionNameCursor.ToListAsync(cancellationToken);

        if (entityCollectionNames.Contains(collectionName))
        {
            return;
        }

        await mongoDatabase.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);

        await mongoCollection.Indexes.CreateManyAsync(collectionIndices, cancellationToken);
    }
}
