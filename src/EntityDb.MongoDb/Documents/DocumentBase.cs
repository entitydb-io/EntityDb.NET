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
    internal abstract record DocumentBase
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        BsonDocumentEnvelope Data,
        [property: BsonIgnoreIfNull]
        ObjectId? _id
    ) : ITransactionDocument
    {
        protected static readonly IndexKeysDefinitionBuilder<BsonDocument> IndexKeys = Builders<BsonDocument>.IndexKeys;

        static DocumentBase()
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        }

        protected static async Task ProvisionCollection
        (
            IMongoDatabase mongoDatabase,
            string collectionName,
            CreateIndexModel<BsonDocument>[] indices
        )
        {
            var entityCollectionNameCursor = await mongoDatabase.ListCollectionNamesAsync();
            var entityCollectionNames = await entityCollectionNameCursor.ToListAsync();

            if (entityCollectionNames.Contains(collectionName) == false)
            {
                await mongoDatabase.CreateCollectionAsync(collectionName);

                var mongoCollection = mongoDatabase.GetCollection<BsonDocument>(collectionName);

                await mongoCollection.Indexes.CreateManyAsync(indices);
            }
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
                    session: clientSessionHandle,
                    document: bsonDocument
                );
        }

        protected static async Task InsertMany<TDocument>
        (
            IClientSessionHandle clientSessionHandle,
            IMongoCollection<BsonDocument> mongoCollection,
            IEnumerable<TDocument> documents
        )
        {
            var bsonDocuments = documents
                .Select(document => document.ToBsonDocument())
                .ToArray();

            if (bsonDocuments.Length == 0)
            {
                return;
            }

            await mongoCollection
                .InsertManyAsync
                (
                    session: clientSessionHandle,
                    documents: bsonDocuments
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
                    session: clientSessionHandle,
                    filter: documentFilter
                );
        }
    }
}
