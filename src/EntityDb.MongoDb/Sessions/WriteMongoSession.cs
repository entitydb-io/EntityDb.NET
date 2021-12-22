using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Transactions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal record WriteMongoSession(IMongoDatabase MongoDatabase, IClientSessionHandle ClientSessionHandle, ILoggerFactory LoggerFactory, IResolvingStrategyChain ResolvingStrategyChain, TransactionSessionOptions TransactionSessionOptions) : IMongoSession
    {
        private IMongoCollection<TDocument> GetMongoCollection<TDocument>(string collectionName)
        {
            return MongoDatabase
                .GetCollection<TDocument>(collectionName)
                .WithReadPreference(ReadPreference.Primary)
                .WithReadConcern(ReadConcern.Majority);
        }

        public async Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments)
        {
            await GetMongoCollection<TDocument>(collectionName)
                .InsertManyAsync
                (
                    ClientSessionHandle,
                    bsonDocuments
                );
        }

        public IFindFluent<TDocument, TDocument> Find<TDocument>(string collectionName,
            FilterDefinition<TDocument> filter)
        {
            return GetMongoCollection<TDocument>(collectionName)
                .Find(ClientSessionHandle, filter, new FindOptions
                {
                    MaxTime = TransactionSessionOptions.ReadTimeout
                });
        }

        public async Task Delete<TDocument>(string collectionName,
            FilterDefinition<TDocument> documentFilter)
        {
            await GetMongoCollection<TDocument>(collectionName)
                .DeleteManyAsync
                (
                    ClientSessionHandle,
                    documentFilter
                );
        }

        public bool TransactionStarted => ClientSessionHandle.IsInTransaction;

        public void StartTransaction()
        {
            ClientSessionHandle.StartTransaction(new TransactionOptions(
                readPreference: ReadPreference.Primary,
                readConcern: ReadConcern.Majority,
                writeConcern: WriteConcern.WMajority,
                maxCommitTime: TransactionSessionOptions.WriteTimeout
            ));
        }

        public async Task CommitTransaction()
        {
#if DEBUG
            await Task.FromException(new TestModeException($"{nameof(CommitTransaction)} should never be called in the DEBUG configuration."));
#else
            await ClientSessionHandle.CommitTransactionAsync();
#endif
        }

        public async Task AbortTransaction()
        {
            await ClientSessionHandle.AbortTransactionAsync();
        }

        public ValueTask DisposeAsync()
        {
            ClientSessionHandle.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
