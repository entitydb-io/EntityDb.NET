using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries
{
    internal record EntityVersionQuery<TDocument> : DocumentQuery<TDocument>
        where TDocument : IEntityDocument
    {
        protected override ProjectionDefinition<BsonDocument, TDocument> Projection =>
            ProjectionBuilder.Combine
            (
                ProjectionBuilder.Exclude(nameof(IEntityDocument._id)),
                ProjectionBuilder.Include(nameof(IEntityDocument.EntityVersionNumber))
            );
    }
}
