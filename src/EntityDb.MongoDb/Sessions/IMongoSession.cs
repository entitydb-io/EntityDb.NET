using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal interface IMongoSession : IDisposable, IAsyncDisposable
    {
        Task InsertOne<TDocument>(IMongoCollection<TDocument> mongoCollection, TDocument bsonDocument);
        Task InsertMany<TDocument>(IMongoCollection<TDocument> mongoCollection, TDocument[] bsonDocuments);

        Task DeleteMany<TDocument>(IMongoCollection<TDocument> mongoCollection,
            FilterDefinition<TDocument> documentFilter);

        IFindFluent<TDocument, TDocument> Find<TDocument>(IMongoCollection<TDocument> mongoCollection,
            FilterDefinition<TDocument> documentFilter);

        void StartTransaction();
        Task CommitTransaction();
        Task AbortTransaction();
    }
}
