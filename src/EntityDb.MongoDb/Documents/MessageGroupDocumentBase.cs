using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Sources.Documents;
using EntityDb.MongoDb.Sources.Queries.FilterBuilders;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Documents;

internal abstract record MessageGroupDocumentBase : DocumentBase, IMessageGroupDocument<BsonDocument>
{
    protected static readonly MessageGroupFilterBuilder FilterBuilder = new();

    public static ProjectionDefinition<BsonDocument> EntityPointersProjection { get; } =
        ProjectionBuilder.Include(nameof(EntityPointers));

    public required Id[] EntityIds { get; init; }
    public required Pointer[] EntityPointers { get; init; }
}
