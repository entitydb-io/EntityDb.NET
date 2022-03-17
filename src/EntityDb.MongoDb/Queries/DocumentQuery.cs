using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Queries;

internal record DocumentQuery<TDocument>
(
    string CollectionName,
    FilterDefinition<BsonDocument> Filter,
    SortDefinition<BsonDocument>? Sort,
    int? Skip,
    int? Limit
)
{
    public Task<List<TDocument>> Execute(IMongoSession mongoSession, ProjectionDefinition<BsonDocument, TDocument> projection, CancellationToken cancellationToken)
    {
        return mongoSession.Find(CollectionName, Filter, projection, Sort, Skip, Limit, cancellationToken);
    }
}
