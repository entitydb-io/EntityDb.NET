using EntityDb.MongoDb.Envelopes;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal record EntityDocumentBase
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        Guid EntityId,
        ulong EntityVersionNumber,
        BsonDocumentEnvelope Data,
        ObjectId? _id = null
    ) : TransactionDocumentBase
    (
        TransactionTimeStamp,
        TransactionId,
        Data,
        _id
    )
    {
        protected static readonly ProjectionDefinition<BsonDocument> _entityIdProjection = Projection.Combine
        (
            Projection.Exclude(nameof(_id)),
            Projection.Include(nameof(EntityId))
        );

        public static async Task<Guid[]> GetEntityIds<TDocument>
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoCollection<BsonDocument> mongoCollection,
            FilterDefinition<BsonDocument> filter,
            SortDefinition<BsonDocument>? sort,
            int? skip,
            int? limit
        )
            where TDocument : EntityDocumentBase
        {
            var documents = await GetMany<TDocument>
            (
                clientSessionHandle,
                mongoCollection,
                filter,
                sort,
                _entityIdProjection
            );

            var entityIds = documents
                .Select(document => document.EntityId)
                .Distinct();

            if (skip.HasValue)
            {
                entityIds = entityIds.Skip(skip.Value);
            }

            if (limit.HasValue)
            {
                entityIds = entityIds.Take(limit.Value);
            }

            return entityIds.ToArray();
        }
    }
}
