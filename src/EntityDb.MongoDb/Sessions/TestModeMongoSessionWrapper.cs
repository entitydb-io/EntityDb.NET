using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Transactions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal sealed record TestModeMongoSessionWrapper(IMongoSession MongoSession, bool ReadOnly) : IMongoSession
    {
        public IMongoDatabase MongoDatabase => MongoSession.MongoDatabase;
        public ILoggerFactory LoggerFactory => MongoSession.LoggerFactory;
        public IResolvingStrategyChain ResolvingStrategyChain => MongoSession.ResolvingStrategyChain;
        public TransactionSessionOptions TransactionSessionOptions => MongoSession.TransactionSessionOptions;

        public async Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments)
        {
            await MongoSession.Insert(collectionName, bsonDocuments);
        }

        public IFindFluent<TDocument, TDocument> Find<TDocument>(string collectionName, FilterDefinition<TDocument> documentFilter)
        {
            return MongoSession.Find(collectionName, documentFilter);
        }

        public async Task Delete<TDocument>(string collectionName, FilterDefinition<TDocument> documentFilter)
        {
            await MongoSession.Delete(collectionName, documentFilter);
        }

        public bool TransactionStarted => MongoSession.TransactionStarted;

        public void StartTransaction()
        {
            if (!ReadOnly && !TransactionStarted)
            {
                throw new TestModeException("Transaction should be started already.");
            }
        }

        public Task CommitTransaction()
        {
            if (!ReadOnly && !TransactionStarted)
            {
                throw new TestModeException("Transaction should be started already.");
            }

            return Task.CompletedTask;
        }

        public Task AbortTransaction()
        {
            if (!ReadOnly && !TransactionStarted)
            {
                throw new TestModeException("Transaction should be started already.");
            }

            return Task.CompletedTask;
        }
    }
}
