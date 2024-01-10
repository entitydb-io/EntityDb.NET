using EntityDb.Abstractions.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.MongoDb.Documents;

internal sealed record StateDocument
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    [BsonIgnoreIfNull] public ObjectId? _id { get; init; }
    public required string DataType { get; init; }
    public required BsonDocument Data { get; init; }
    public required Id StateId { get; init; }
    public required Version StateVersion { get; init; }
    public required Pointer StatePointer { get; init; }
}
