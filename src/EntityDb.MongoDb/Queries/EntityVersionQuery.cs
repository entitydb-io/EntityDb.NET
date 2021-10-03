using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries
{
    internal record EntityVersionQuery<TDocument>
    (
        FilterDefinition<BsonDocument> Filter,
        SortDefinition<BsonDocument>? Sort,
        int? Skip,
        int? Limit
    )
        : DocumentQuery<TDocument>
    (
        Filter,
        _projection,
        Sort,
        Skip,
        Limit
    )
        where TDocument : IEntityDocument
    {
        private static readonly ProjectionDefinition<BsonDocument> _projection = _projectionBuilder.Combine
        (
            _projectionBuilder.Exclude(nameof(IEntityDocument._id)),
            _projectionBuilder.Include(nameof(IEntityDocument.EntityVersionNumber))
        );
    }
}
