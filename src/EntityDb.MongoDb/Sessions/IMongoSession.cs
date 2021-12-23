using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Transactions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal interface IMongoSession : IDisposableResource
    {
        IMongoDatabase MongoDatabase { get; }
        ILogger Logger { get; }
        IResolvingStrategyChain ResolvingStrategyChain { get; }

        Task Insert<TDocument>(string collectionName,
            TDocument[] bsonDocuments);
        IFindFluent<TDocument, TDocument> Find<TDocument>(string collectionName,
            FilterDefinition<TDocument> documentFilter);
        Task Delete<TDocument>(string collectionName,
            FilterDefinition<TDocument> documentFilter);

        void StartTransaction();
        Task CommitTransaction();
        Task AbortTransaction();

        IMongoSession WithTransactionSessionOptions(TransactionSessionOptions transactionSessionOptions);
    }
}
