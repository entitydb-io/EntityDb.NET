using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Queries
{
    internal record DocumentQuery<TDocument> : DocumentQuery
        where TDocument : ITransactionDocument
    {
        protected virtual ProjectionDefinition<BsonDocument, TDocument> Projection =>
            ProjectionBuilder.Exclude(nameof(ITransactionDocument._id));

        public IClientSessionHandle? ClientSessionHandle { get; init; }
        public IMongoCollection<BsonDocument> MongoCollection { get; init; } = default!;
        public FilterDefinition<BsonDocument> Filter { get; init; } = default!;
        public SortDefinition<BsonDocument>? Sort { get; init; }
        public int? Skip { get; init; }
        public int? Limit { get; init; }

        public Task<List<TDocument>> GetDocuments()
        {
            var find = ClientSessionHandle != null
                ? MongoCollection.Find(ClientSessionHandle, Filter).Project(Projection)
                : MongoCollection.Find(Filter).Project(Projection);

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
