using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.MongoDb.Queries
{
    internal record EntityIdsQuery<TDocument>
    (
        IClientSessionHandle? ClientSessionHandle,
        IMongoCollection<BsonDocument> MongoCollection,
        FilterDefinition<BsonDocument> Filter,
        SortDefinition<BsonDocument>? Sort,
        int? Skip,
        int? Limit
    )
        : GuidQuery<TDocument>
    (
        ClientSessionHandle,
        MongoCollection,
        Filter,
        _projection,
        Sort,
        Skip,
        Limit
    )
        where TDocument : IEntitiesDocument
    {
        protected static readonly ProjectionDefinition<BsonDocument> _projection = _projectionBuilder.Combine
        (
            _projectionBuilder.Exclude(nameof(IEntitiesDocument._id)),
            _projectionBuilder.Include(nameof(IEntitiesDocument.EntityIds))
        );

        protected override IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents) => documents.SelectMany(document => document.EntityIds);
    }
}
