using EntityDb.Abstractions.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EntityDb.MongoDb.Documents;

internal sealed record SnapshotDocument
{
    [BsonIgnoreIfNull] public ObjectId? _id { get; init; }
    public string DataType { get; init; } = default!;
    public BsonDocument Data { get; init; } = default!;
    public Id PointerId { get; init; }
    public VersionNumber PointerVersionNumber { get; init; }
}
