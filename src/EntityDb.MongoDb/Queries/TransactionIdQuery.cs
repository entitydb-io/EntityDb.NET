using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.MongoDb.Queries
{
    internal record TransactionIdQuery<TDocument> : GuidQuery<TDocument>
        where TDocument : ITransactionDocument
    {
        public override ProjectionDefinition<BsonDocument, TDocument> Projection { get; init; } = _projectionBuilder.Combine
        (
            _projectionBuilder.Exclude(nameof(ITransactionDocument._id)),
            _projectionBuilder.Include(nameof(ITransactionDocument.TransactionId))
        );

        protected override IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents) => documents.Select(document => document.TransactionId);
    }
}
