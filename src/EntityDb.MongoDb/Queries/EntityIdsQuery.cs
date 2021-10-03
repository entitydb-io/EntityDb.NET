using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.MongoDb.Queries
{
    internal record EntityIdsQuery<TDocument> : GuidQuery<TDocument>
        where TDocument : IEntitiesDocument
    {
        public override ProjectionDefinition<BsonDocument, TDocument> Projection { get; init; } = _projectionBuilder.Combine
        (
            _projectionBuilder.Exclude(nameof(IEntitiesDocument._id)),
            _projectionBuilder.Include(nameof(IEntitiesDocument.EntityIds))
        );

        protected override IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents) => documents.SelectMany(document => document.EntityIds);
    }
}
