using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal abstract record DocumentBase : ITransactionDocument
    {
        static DocumentBase()
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        }

        public DateTime TransactionTimeStamp { get; init; }

        [BsonIgnoreIfNull] public ObjectId? _id { get; init; }

        public Guid TransactionId { get; init; }
        public BsonDocumentEnvelope Data { get; init; } = default!;

        public static IMongoCollection<BsonDocument> GetMongoCollection
        (
            IMongoDatabase mongoDatabase,
            string collectionName
        )
        {
            return mongoDatabase.GetCollection<BsonDocument>(collectionName);
        }

        protected static async Task InsertOne<TDocument>
        (
            IMongoSession mongoSession,
            IMongoCollection<BsonDocument> mongoCollection,
            TDocument document
        )
        {
            var bsonDocument = document.ToBsonDocument();

            await mongoSession.InsertOne(mongoCollection, bsonDocument);
        }

        protected static async Task InsertMany<TDocument>
        (
            IMongoSession mongoSession,
            IMongoCollection<BsonDocument> mongoCollection,
            IReadOnlyCollection<TDocument> documents
        )
        {
            var bsonDocuments = documents
                .Select(document => document.ToBsonDocument())
                .ToArray();

            await mongoSession.InsertMany(mongoCollection, bsonDocuments);
        }

        protected static async Task DeleteMany
        (
            IMongoSession mongoSession,
            IMongoCollection<BsonDocument> mongoCollection,
            FilterDefinition<BsonDocument> documentFilter
        )
        {
            await mongoSession.DeleteMany(mongoCollection, documentFilter);
        }
    }
}
