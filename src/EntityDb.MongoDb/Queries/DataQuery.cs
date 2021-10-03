using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries
{
    internal record DataQuery<TDocument>
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
        where TDocument : DocumentBase
    {
        private static readonly ProjectionDefinition<BsonDocument, TDocument> _projection = _projectionBuilder.Combine
        (
            _projectionBuilder.Exclude(nameof(DocumentBase._id)),
            _projectionBuilder.Include(nameof(DocumentBase.Data))
        );
    }
}
