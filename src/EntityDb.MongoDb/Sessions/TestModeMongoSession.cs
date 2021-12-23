using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Transactions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal record TestModeMongoSession(IMongoSession MongoSession) : IMongoSession
    {
        public IMongoDatabase MongoDatabase => MongoSession.MongoDatabase;
        public ILogger Logger => MongoSession.Logger;
        public IResolvingStrategyChain ResolvingStrategyChain => MongoSession.ResolvingStrategyChain;

        public Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments)
        {
            return MongoSession.Insert(collectionName, bsonDocuments);
        }

        public IFindFluent<TDocument, TDocument> Find<TDocument>(string collectionName,
            FilterDefinition<TDocument> filter)
        {
            return MongoSession.Find(collectionName, filter);
        }

        public Task Delete<TDocument>(string collectionName,
            FilterDefinition<TDocument> documentFilter)
        {
            return MongoSession.Delete(collectionName, documentFilter);
        }

        public IMongoSession WithTransactionSessionOptions(TransactionSessionOptions transactionSessionOptions)
        {
            return this with
            {
                MongoSession = MongoSession.WithTransactionSessionOptions(transactionSessionOptions),
            };
        }

        public void StartTransaction()
        {
        }
        
        public Task CommitTransaction()
        {
            return Task.CompletedTask;
        }

        public Task AbortTransaction()
        {
            return Task.CompletedTask;
        }
    }
}
