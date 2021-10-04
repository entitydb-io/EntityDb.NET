using EntityDb.MongoDb.Envelopes;
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

        protected static Task InsertOne<TDocument>
        (
            IClientSessionHandle clientSessionHandle,
            IMongoCollection<BsonDocument> mongoCollection,
            TDocument document
        )
        {
            var bsonDocument = document.ToBsonDocument();

            return mongoCollection
                .InsertOneAsync
                (
                    clientSessionHandle,
                    bsonDocument
                );
        }

        protected static async Task InsertMany<TDocument>
        (
            IClientSessionHandle clientSessionHandle,
            IMongoCollection<BsonDocument> mongoCollection,
            IReadOnlyCollection<TDocument> documents
        )
        {
            if (documents.Count == 0)
            {
                return;
            }

            var bsonDocuments = documents
                .Select(document => document.ToBsonDocument())
                .ToArray();

            await mongoCollection
                .InsertManyAsync
                (
                    clientSessionHandle,
                    bsonDocuments
                );
        }

        protected static Task DeleteMany
        (
            IClientSessionHandle clientSessionHandle,
            IMongoCollection<BsonDocument> mongoCollection,
            FilterDefinition<BsonDocument> documentFilter
        )
        {
            return mongoCollection
                .DeleteManyAsync
                (
                    clientSessionHandle,
                    documentFilter
                );
        }
    }
}
