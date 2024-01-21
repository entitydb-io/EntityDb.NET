using EntityDb.Abstractions;
using EntityDb.Abstractions.States;
using EntityDb.Common.Sources.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Documents;

internal abstract record MessageDataDocumentBase : DocumentBase, IMessageDataDocument<BsonDocument>
{
    public static ProjectionDefinition<BsonDocument> StatePointerProjection { get; } =
        ProjectionBuilder.Include(nameof(StatePointer));

    public required Id StateId { get; init; }
    public required StateVersion StateVersion { get; init; }

    public required Id MessageId { get; init; }
    public required StatePointer StatePointer { get; init; }
}
