using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.MongoDb.Queries
{
    internal record EntityIdQuery<TDocument> : GuidQuery<TDocument>
        where TDocument : IEntityDocument
    {
        protected override ProjectionDefinition<BsonDocument, TDocument> Projection =>
            ProjectionBuilder.Combine
            (
                ProjectionBuilder.Exclude(nameof(IEntityDocument._id)),
                ProjectionBuilder.Include(nameof(IEntityDocument.EntityId))
            );

        protected override IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents)
        {
            return documents.Select(document => document.EntityId);
        }
    }
}
