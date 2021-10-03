using EntityDb.MongoDb.Documents;
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
        FilterDefinition<BsonDocument> Filter,
        ProjectionDefinition<BsonDocument, TDocument> Projection,
        SortDefinition<BsonDocument>? Sort,
        int? DistinctSkip,
        int? DistinctLimit
    )
        : DocumentQuery<TDocument>
    (
        Filter,
        Projection,
        Sort,
        null,
        null
    )
        where TDocument : ITransactionDocument
    {
        protected abstract IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents);

        public async Task<Guid[]> DistinctGuids
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoCollection<BsonDocument> mongoCollection
        )
        {
            var documents = await Execute(clientSessionHandle, mongoCollection);

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
