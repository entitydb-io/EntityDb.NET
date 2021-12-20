using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal record MongoSession(IClientSessionHandle ClientSessionHandle) : IMongoSession
    {
        public Task InsertOne<TDocument>(IMongoCollection<TDocument> mongoCollection, TDocument bsonDocument)
        {
            return mongoCollection
                .InsertOneAsync
                (
                    ClientSessionHandle,
                    bsonDocument
                );
        }

        public async Task InsertMany<TDocument>(IMongoCollection<TDocument> mongoCollection, TDocument[] bsonDocuments)
        {
            if (bsonDocuments.Length == 0)
            {
                return;
            }

            await mongoCollection
                .InsertManyAsync
                (
                    ClientSessionHandle,
                    bsonDocuments
                );
        }

        public async Task DeleteMany<TDocument>(IMongoCollection<TDocument> mongoCollection, FilterDefinition<TDocument> documentFilter)
        {
            await mongoCollection
                .DeleteManyAsync
                (
                    ClientSessionHandle,
                    documentFilter
                );
        }

        public IFindFluent<TDocument, TDocument> Find<TDocument>(IMongoCollection<TDocument> mongoCollection, FilterDefinition<TDocument> filter)
        {
            return mongoCollection.Find(ClientSessionHandle, filter);
        }

        public virtual void StartTransaction()
        {
            ClientSessionHandle.StartTransaction();
        }

        public virtual async Task CommitTransaction()
        {
            await ClientSessionHandle.CommitTransactionAsync();
        }

        public virtual async Task AbortTransaction()
        {
            await ClientSessionHandle.AbortTransactionAsync();
        }

        [ExcludeFromCodeCoverage(Justification = "Proxy for DisposeAsync")]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public virtual async ValueTask DisposeAsync()
        {
            await Task.Yield();

            ClientSessionHandle.Dispose();
        }
    }
}
