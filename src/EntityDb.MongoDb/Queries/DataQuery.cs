using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Queries
{
    internal record DataQuery<TDocument> : DocumentQuery<TDocument>
        where TDocument : ITransactionDocument
    {
        protected override ProjectionDefinition<BsonDocument, TDocument> Projection => ProjectionBuilder.Combine
            (
                ProjectionBuilder.Exclude(nameof(ITransactionDocument._id)),
                ProjectionBuilder.Include(nameof(ITransactionDocument.Data))
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
