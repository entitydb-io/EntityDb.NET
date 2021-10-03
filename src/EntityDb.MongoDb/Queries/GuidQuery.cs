using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Queries
{
    internal abstract record GuidQuery<TDocument>
(
        IClientSessionHandle? ClientSessionHandle,
        IMongoCollection<BsonDocument> MongoCollection,
        FilterDefinition<BsonDocument> Filter,
        ProjectionDefinition<BsonDocument, TDocument> Projection,
        SortDefinition<BsonDocument>? Sort,
        int? DistinctSkip,
        int? DistinctLimit
    )
        : DocumentQuery<TDocument>
    (
        ClientSessionHandle,
        MongoCollection,
        Filter,
        Projection,
        Sort,
        null,
        null
    )
    {
        protected abstract IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents);

        public async Task<Guid[]> GetDistinctGuids()
        {
            var documents = await GetDocuments();

            var guids = MapToGuids(documents).Distinct();

            if (DistinctSkip.HasValue)
            {
                guids = guids.Skip(DistinctSkip.Value);
            }

            if (DistinctLimit.HasValue)
            {
                guids = guids.Take(DistinctLimit.Value);
            }

            return guids.ToArray();
        }
    }
}
