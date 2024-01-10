using EntityDb.MongoDb.Sources.Queries;
using EntityDb.MongoDb.Sources.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Documents.Queries;

internal record DocumentQuery<TDocument>
{
    public required string CollectionName { get; init; }
    public required FilterDefinition<BsonDocument> Filter { get; init; }
    public required SortDefinition<BsonDocument>? Sort { get; init; }
    public required int? Skip { get; init; }
    public required int? Limit { get; init; }
    public required MongoDbQueryOptions? Options { get; init; }

    public IAsyncEnumerable<TDocument> Execute(IMongoSession mongoSession,
        ProjectionDefinition<BsonDocument, TDocument> projection, CancellationToken cancellationToken)
    {
        return mongoSession.Find(CollectionName, Filter, projection, Sort, Skip, Limit, Options, cancellationToken);
    }
}
