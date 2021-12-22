using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Transactions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal record ReadOnlyMongoSession(IMongoDatabase MongoDatabase, ILoggerFactory LoggerFactory, IResolvingStrategyChain ResolvingStrategyChain, TransactionSessionOptions TransactionSessionOptions) : IMongoSession
    {
        //TODO: Cover this with base transaction repository tests
        private IMongoCollection<TDocument> GetMongoCollection<TDocument>(string collectionName)
        {
            if (TransactionSessionOptions.SecondaryPreferred)
            {
                return MongoDatabase
                    .GetCollection<TDocument>(collectionName)
                    .WithReadPreference(ReadPreference.SecondaryPreferred)
                    .WithReadConcern(ReadConcern.Available);
            }

            return MongoDatabase
                .GetCollection<TDocument>(collectionName)
                .WithReadPreference(ReadPreference.PrimaryPreferred)
                .WithReadConcern(ReadConcern.Majority);
        }

        public Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments)
        {
            throw new CannotWriteInReadOnlyModeException();
        }

        public IFindFluent<TDocument, TDocument> Find<TDocument>(string collectionName,
            FilterDefinition<TDocument> filter)
        {
            return GetMongoCollection<TDocument>(collectionName)
                .Find(filter, new FindOptions
                {
                    MaxTime = TransactionSessionOptions.ReadTimeout
                });
        }

        public Task Delete<TDocument>(string collectionName,
            FilterDefinition<TDocument> documentFilter)
        {
            throw new CannotWriteInReadOnlyModeException();
        }

        public bool TransactionStarted => false;

        public virtual void StartTransaction()
        {
            throw new CannotWriteInReadOnlyModeException();
        }

        public Task CommitTransaction()
        {
            throw new CannotWriteInReadOnlyModeException();
        }

        public Task AbortTransaction()
        {
            throw new CannotWriteInReadOnlyModeException();
        }
    }
}
