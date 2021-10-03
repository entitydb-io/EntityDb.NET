using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Queries
{
    internal record DocumentQuery<TDocument>
    (
        IClientSessionHandle? ClientSessionHandle,
        IMongoCollection<BsonDocument> MongoCollection,
        FilterDefinition<BsonDocument> Filter,
        ProjectionDefinition<BsonDocument, TDocument> Projection,
        SortDefinition<BsonDocument>? Sort,
        int? Skip,
        int? Limit
    )
    {
        protected static readonly ProjectionDefinitionBuilder<BsonDocument> _projectionBuilder = Builders<BsonDocument>.Projection;

        public Task<List<TDocument>> GetDocuments()
        {
            IFindFluent<BsonDocument, TDocument> find;

            if (ClientSessionHandle != null)
            {
                find = MongoCollection
                    .Find(ClientSessionHandle, Filter)
                    .Project(Projection);
            }
            else
            {
                find = MongoCollection
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
