using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Queries
{
    internal record DocumentQuery<TDocument>
    (
        FilterDefinition<BsonDocument> Filter,
        ProjectionDefinition<BsonDocument, TDocument> Projection,
        SortDefinition<BsonDocument>? Sort,
        int? Skip,
        int? Limit
    )
    {
        protected static readonly ProjectionDefinitionBuilder<BsonDocument> _projectionBuilder = Builders<BsonDocument>.Projection;

        public Task<List<TDocument>> Execute
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoCollection<BsonDocument> mongoCollection
        )
        {
            IFindFluent<BsonDocument, TDocument> find;

            if (clientSessionHandle != null)
            {
                find = mongoCollection
                    .Find(clientSessionHandle, Filter)
                    .Project(Projection);
            }
            else
            {
                find = mongoCollection
                    .Find(Filter)
                    .Project(Projection);
            }

            if (Sort != null)
            {
                find = find.Sort(Sort);
            }

            if (Skip != null)
            {
                find = find.Skip(Skip);
            }

            if (Limit != null)
            {
                find = find.Limit(Limit);
            }

            return find.ToListAsync();
        }
    }
}
