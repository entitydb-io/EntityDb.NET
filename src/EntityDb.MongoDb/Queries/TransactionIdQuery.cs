using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.MongoDb.Queries
{
    internal record TransactionIdQuery<TDocument>
    (
        FilterDefinition<BsonDocument> Filter,
        SortDefinition<BsonDocument>? Sort,
        int? Skip,
        int? Limit
    )
        : GuidQuery<TDocument>
    (
        Filter,
        _projection,
        Sort,
        Skip,
        Limit
    )
        where TDocument : ITransactionDocument
    {
        protected static readonly ProjectionDefinition<BsonDocument> _projection = _projectionBuilder.Combine
        (
            _projectionBuilder.Exclude(nameof(ITransactionDocument._id)),
            _projectionBuilder.Include(nameof(ITransactionDocument.TransactionId))
        );

        protected override IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents) => documents.Select(document => document.TransactionId);
    }
}
