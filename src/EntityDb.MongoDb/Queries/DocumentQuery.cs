using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries
{
    internal record DocumentQuery
    {
        protected static readonly ProjectionDefinitionBuilder<BsonDocument> ProjectionBuilder =
            Builders<BsonDocument>.Projection;
    }
}
