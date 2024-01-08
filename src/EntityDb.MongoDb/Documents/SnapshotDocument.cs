using EntityDb.Abstractions.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.MongoDb.Documents;

internal sealed record SnapshotDocument
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    [BsonIgnoreIfNull] public ObjectId? _id { get; init; }
    public required string DataType { get; init; }
    public required BsonDocument Data { get; init; }
    public required Id SnapshotId { get; init; }
    public required Version SnapshotVersion { get; init; }
    public required Pointer SnapshotPointer { get; init; }
}
