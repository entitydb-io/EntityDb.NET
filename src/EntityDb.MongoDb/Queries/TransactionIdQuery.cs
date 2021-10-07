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
        protected override ProjectionDefinition<BsonDocument, TDocument> Projection =>
            ProjectionBuilder.Combine
            (
                ProjectionBuilder.Exclude(nameof(ITransactionDocument._id)),
                ProjectionBuilder.Include(nameof(ITransactionDocument.TransactionId))
            );

        protected override IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents)
        {
            return documents.Select(document => document.TransactionId);
        }
    }
}
