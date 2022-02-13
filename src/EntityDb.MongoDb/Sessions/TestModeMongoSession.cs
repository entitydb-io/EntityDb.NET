﻿using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.TypeResolvers;
using EntityDb.Common.Transactions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal record TestModeMongoSession(IMongoSession MongoSession) : IMongoSession
    {
        public IMongoDatabase MongoDatabase => MongoSession.MongoDatabase;
        public ILogger Logger => MongoSession.Logger;
        public ITypeResolver TypeResolver => MongoSession.TypeResolver;

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
            // Test Mode Transactions are started in the Test Mode Repository Factory
        }

        public Task CommitTransaction()
        {
            // Test Mode Transactions are never committed
            return Task.CompletedTask;
        }

        public Task AbortTransaction()
        {
            // Test Mode Transactions are aborted in the Test Mode Repository Factory
            return Task.CompletedTask;
        }
    }
}
