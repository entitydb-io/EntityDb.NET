using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Sources.Documents;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Documents;

internal abstract record DocumentBase : IDocument<BsonDocument>
{
    protected static readonly ProjectionDefinitionBuilder<BsonDocument> ProjectionBuilder =
        Builders<BsonDocument>.Projection;

    public static ProjectionDefinition<BsonDocument> NoIdProjection { get; } =
        ProjectionBuilder.Exclude(nameof(_id));

    public static ProjectionDefinition<BsonDocument> SourceIdProjection { get; } =
        ProjectionBuilder.Include(nameof(SourceId));

    public static ProjectionDefinition<BsonDocument> DataProjection { get; } =
        ProjectionBuilder.Combine
        (
            ProjectionBuilder.Exclude(nameof(_id)),
            ProjectionBuilder.Include(nameof(Data))
        );

    // ReSharper disable once InconsistentNaming
    [BsonIgnoreIfNull] public ObjectId? _id { get; init; }
    public required string DataType { get; init; }
    public required Id SourceId { get; init; }
    public required TimeStamp SourceTimeStamp { get; init; }
    public required BsonDocument Data { get; init; }
}
