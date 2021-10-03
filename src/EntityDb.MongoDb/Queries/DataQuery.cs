using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Queries
{
    internal record DataQuery<TDocument>
    (
        IClientSessionHandle? ClientSessionHandle,
        IMongoCollection<BsonDocument> MongoCollection,
        FilterDefinition<BsonDocument> Filter,
        SortDefinition<BsonDocument>? Sort,
        int? Skip,
        int? Limit
    )
        : DocumentQuery<TDocument>
    (
        ClientSessionHandle,
        MongoCollection,
        Filter,
        _projection,
        Sort,
        Skip,
        Limit
    )
        where TDocument : ITransactionDocument
    {
        private static readonly ProjectionDefinition<BsonDocument, TDocument> _projection = _projectionBuilder.Combine
        (
            _projectionBuilder.Exclude(nameof(ITransactionDocument._id)),
            _projectionBuilder.Include(nameof(ITransactionDocument.Data))
        );

        public async Task<TModel[]> GetModels<TModel>(ILogger logger, IResolvingStrategyChain resolvingStrategyChain)
        {
            var documents = await GetDocuments();

            return documents
                .Select(document => document.Data.Reconstruct<TModel>(logger, resolvingStrategyChain))
                .ToArray();
        }
    }
}
