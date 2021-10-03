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
    internal abstract record TransactionDocumentBase
    (
        DateTime TransactionTimeStamp,
        Guid TransactionId,
        BsonDocumentEnvelope Data,
#pragma warning disable IDE1006 // Naming Styles
        [property: BsonIgnoreIfNull]
        ObjectId? _id
#pragma warning restore IDE1006 // Naming Styles
    )
    {
        protected static readonly ProjectionDefinitionBuilder<BsonDocument> Projection = Builders<BsonDocument>.Projection;
        protected static readonly IndexKeysDefinitionBuilder<BsonDocument> IndexKeys = Builders<BsonDocument>.IndexKeys;

        protected static readonly ProjectionDefinition<BsonDocument> TransactionIdProjection = Projection.Combine
        (
            Projection.Exclude(nameof(_id)),
            Projection.Include(nameof(TransactionId))
        );

        protected static readonly ProjectionDefinition<BsonDocument> DataProjection = Projection.Combine
        (
            Projection.Exclude(nameof(_id)),
            Projection.Include(nameof(Data))
        );

        static TransactionDocumentBase()
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

        protected static Task<List<TDocument>> GetMany<TDocument>
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoCollection<BsonDocument> mongoCollection,
            FilterDefinition<BsonDocument> filter,
            SortDefinition<BsonDocument>? sort,
            ProjectionDefinition<BsonDocument, TDocument> projection,
            int? skip = null,
            int? limit = null
        )
        {
            IFindFluent<BsonDocument, TDocument> query;

            if (clientSessionHandle != null)
            {
                query = mongoCollection
                    .Find(clientSessionHandle, filter)
                    .Project(projection);
            }
            else
            {
                query = mongoCollection
                    .Find(filter)
                    .Project(projection);
            }

            if (sort != null)
            {
                query = query.Sort(sort);
            }

            if (skip != null)
            {
                query = query.Skip(skip);
            }

            if (limit != null)
            {
                query = query.Limit(limit);
            }

            return query.ToListAsync();
        }

        protected static async Task<Guid[]> GetTransactionIds<TDocument>
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoCollection<BsonDocument> mongoCollection,
            FilterDefinition<BsonDocument> filter,
            SortDefinition<BsonDocument>? sort,
            int? skip,
            int? limit
        )
            where TDocument : TransactionDocumentBase
        {
            var documents = await GetMany<TDocument>
            (
                clientSessionHandle,
                mongoCollection,
                filter,
                sort,
                TransactionIdProjection
            );

            var transactionIds = documents
                .Select(document => document.TransactionId)
                .Distinct();

            if (skip.HasValue)
            {
                transactionIds = transactionIds.Skip(skip.Value);
            }

            if (limit.HasValue)
            {
                transactionIds = transactionIds.Take(limit.Value);
            }

            return transactionIds.ToArray();
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
