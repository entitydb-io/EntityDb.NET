using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Sources.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.MongoDb.Documents;

internal abstract record MessageDataDocumentBase : DocumentBase, IMessageDataDocument<BsonDocument>
{
    public static ProjectionDefinition<BsonDocument> StatePointerProjection { get; } =
        ProjectionBuilder.Include(nameof(StatePointer));

    public required Id StateId { get; init; }
    public required Version StateVersion { get; init; }

    public required Id MessageId { get; init; }
    public required Pointer StatePointer { get; init; }
}
