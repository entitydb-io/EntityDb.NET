using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries
{
    internal record EntityVersionQuery<TDocument> : DocumentQuery<TDocument>
        where TDocument : IEntityDocument
    {
        public override ProjectionDefinition<BsonDocument, TDocument> Projection { get; init; } = _projectionBuilder.Combine
        (
            _projectionBuilder.Exclude(nameof(IEntityDocument._id)),
            _projectionBuilder.Include(nameof(IEntityDocument.EntityVersionNumber))
        );
    }
}
