using EntityDb.Abstractions;
using EntityDb.Abstractions.States;
using EntityDb.Common.Sources.Documents;
using EntityDb.MongoDb.Sources.Queries.FilterBuilders;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Documents;

internal abstract record SourceDataDocumentBase : DocumentBase, ISourceDataDocument<BsonDocument>
{
    protected static readonly SourceDataFilterBuilder DataFilterBuilder = new();

    public static ProjectionDefinition<BsonDocument> StatePointersProjection { get; } =
        ProjectionBuilder.Include(nameof(StatePointers));

    public required Id[] StateIds { get; init; }

    public required Id[] MessageIds { get; init; }
    public required StatePointer[] StatePointers { get; init; }
}
