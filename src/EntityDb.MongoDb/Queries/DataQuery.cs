using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Queries
{
    internal record DataQuery<TDocument> : DocumentQuery<TDocument>
        where TDocument : ITransactionDocument
    {
        public override ProjectionDefinition<BsonDocument, TDocument> Projection { get; init; } =
            _projectionBuilder.Combine
            (
                _projectionBuilder.Exclude(nameof(ITransactionDocument._id)),
                _projectionBuilder.Include(nameof(ITransactionDocument.Data))
            );

        public async Task<TModel[]> GetModels<TModel>(ILogger logger, IResolvingStrategyChain resolvingStrategyChain)
        {
            List<TDocument>? documents = await GetDocuments();

            return documents
                .Select(document => document.Data.Reconstruct<TModel>(logger, resolvingStrategyChain))
                .ToArray();
        }
    }
}
