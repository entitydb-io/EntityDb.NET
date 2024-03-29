﻿using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries;

internal record DocumentQuery<TDocument>
(
    string CollectionName,
    FilterDefinition<BsonDocument> Filter,
    SortDefinition<BsonDocument>? Sort,
    int? Skip,
    int? Limit,
    MongoDbQueryOptions? Options
)
{
    public IAsyncEnumerable<TDocument> Execute(IMongoSession mongoSession, ProjectionDefinition<BsonDocument, TDocument> projection, CancellationToken cancellationToken)
    {
        return mongoSession.Find(CollectionName, Filter, projection, Sort, Skip, Limit, Options, cancellationToken);
    }
}
