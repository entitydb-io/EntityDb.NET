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
        protected override ProjectionDefinition<BsonDocument, TDocument> Projection =>
            ProjectionBuilder.Combine
            (
                ProjectionBuilder.Exclude(nameof(IEntitiesDocument._id)),
                ProjectionBuilder.Include(nameof(IEntitiesDocument.EntityIds))
            );

        protected override IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents)
        {
            return documents.SelectMany(document => document.EntityIds);
        }
    }
}
