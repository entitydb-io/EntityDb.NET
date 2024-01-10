using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Sources.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.MongoDb.Documents;

internal abstract record MessageDocumentBase : DocumentBase, IMessageDocument<BsonDocument>
{
    public static ProjectionDefinition<BsonDocument> EntityPointerProjection { get; } =
        ProjectionBuilder.Include(nameof(EntityPointer));

    public required Id MessageId { get; init; }
    
    public required Id EntityId { get; init; }
    public required Version EntityVersion { get; init; }
    public required Pointer EntityPointer { get; init; }
}
