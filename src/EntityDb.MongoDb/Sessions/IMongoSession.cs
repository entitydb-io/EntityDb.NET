using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.Common.Transactions;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal interface IMongoSession : IDisposable, IAsyncDisposable
    {
        IMongoDatabase MongoDatabase { get; }
        ILoggerFactory LoggerFactory { get; }
        IResolvingStrategyChain ResolvingStrategyChain { get; }
        TransactionSessionOptions TransactionSessionOptions { get; }

        public ILogger Logger => TransactionSessionOptions.LoggerOverride ?? LoggerFactory.CreateLogger<IMongoSession>();

        Task Insert<TDocument>(string collectionName,
            TDocument[] bsonDocuments);
        IFindFluent<TDocument, TDocument> Find<TDocument>(string collectionName,
            FilterDefinition<TDocument> documentFilter);
        Task Delete<TDocument>(string collectionName,
            FilterDefinition<TDocument> documentFilter);


        bool TransactionStarted { get; }

        void StartTransaction();
        Task CommitTransaction();
        Task AbortTransaction();
    }
}
