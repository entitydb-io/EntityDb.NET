﻿using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Transactions;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal record MongoSession
    (
        IMongoDatabase MongoDatabase,
        IClientSessionHandle ClientSessionHandle,
        ILogger Logger,
        IResolvingStrategyChain ResolvingStrategyChain,
        TransactionSessionOptions TransactionSessionOptions
    ) : IMongoSession
    {
        private static readonly WriteConcern WriteConcern = WriteConcern.WMajority;

        private static ReadPreference GetReadPreference(TransactionSessionOptions transactionSessionOptions)
        {
            if (!transactionSessionOptions.ReadOnly)
            {
                return ReadPreference.Primary;
            }

            return transactionSessionOptions.SecondaryPreferred
                ? ReadPreference.SecondaryPreferred
                : ReadPreference.PrimaryPreferred;
        }

        private static ReadConcern GetReadConcern(TransactionSessionOptions transactionSessionOptions)
        {
            if (!transactionSessionOptions.ReadOnly)
            {
                return ReadConcern.Majority;
            }

            return transactionSessionOptions.SecondaryPreferred
                ? ReadConcern.Available
                : ReadConcern.Majority;
        }

        private void AssertNotReadOnly()
        {
            if (TransactionSessionOptions.ReadOnly)
            {
                throw new CannotWriteInReadOnlyModeException();
            }
        }

        public async Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments)
        {
            AssertNotReadOnly();

            await MongoDatabase
                .GetCollection<TDocument>(collectionName)
                .InsertManyAsync
                (
                    ClientSessionHandle,
                    bsonDocuments
                );
        }

        public IFindFluent<TDocument, TDocument> Find<TDocument>(string collectionName,
            FilterDefinition<TDocument> filter)
        {
            return MongoDatabase
                .GetCollection<TDocument>(collectionName)
                .WithReadPreference(GetReadPreference(TransactionSessionOptions))
                .WithReadConcern(GetReadConcern(TransactionSessionOptions))
                .Find(ClientSessionHandle, filter, new FindOptions
                {
                    MaxTime = TransactionSessionOptions.ReadTimeout
                });
        }

        public async Task Delete<TDocument>(string collectionName,
            FilterDefinition<TDocument> documentFilter)
        {
            AssertNotReadOnly();

            await MongoDatabase
                .GetCollection<TDocument>(collectionName)
                .DeleteManyAsync
                (
                    ClientSessionHandle,
                    documentFilter
                );
        }

        public IMongoSession WithTransactionSessionOptions(TransactionSessionOptions transactionSessionOptions)
        {
            return this with
            {
                TransactionSessionOptions = transactionSessionOptions,
            };
        }

        public void StartTransaction()
        {
            AssertNotReadOnly();

            ClientSessionHandle.StartTransaction(new TransactionOptions
            (
                writeConcern: WriteConcern,
                maxCommitTime: TransactionSessionOptions.WriteTimeout
            ));
        }
        
        [ExcludeFromCodeCoverage(Justification = "Tests should run witht he Debug configuration, and should not execute this method.")]
        public async Task CommitTransaction()
        {
#if DEBUG
            //TODO: CannotCommitInTestModeException
            await Task.FromException(new CannotWriteInReadOnlyModeException());
#else
            AssertNotReadOnly();

            await ClientSessionHandle.CommitTransactionAsync();
#endif
        }

        public async Task AbortTransaction()
        {
            AssertNotReadOnly();

            await ClientSessionHandle.AbortTransactionAsync();
        }
        
        public ValueTask DisposeAsync()
        {
            ClientSessionHandle.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}