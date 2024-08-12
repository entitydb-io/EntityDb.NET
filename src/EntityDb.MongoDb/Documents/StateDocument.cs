using EntityDb.Abstractions;
using EntityDb.Abstractions.States;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EntityDb.MongoDb.Documents;

internal sealed record StateDocument
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    [BsonIgnoreIfNull] public ObjectId? _id { get; init; }
    public required string DataType { get; init; }
    public required BsonDocument Data { get; init; }
    public required Id StateId { get; init; }
    public required StateVersion StateVersion { get; init; }
    public required StatePointer StatePointer { get; init; }
}
