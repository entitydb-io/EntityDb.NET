﻿using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class MongoDbTransactionRepositoryFactory<TEntity> : IMongoDbTransactionRepositoryFactory<TEntity>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptionsFactory<TransactionSessionOptions> _optionsFactory;
        private readonly IResolvingStrategyChain _resolvingStrategyChain;
        private readonly string _connectionString;
        private readonly string _databaseName;

        public MongoDbTransactionRepositoryFactory
        (
            IOptionsFactory<TransactionSessionOptions> optionsFactory,
            ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain,
            string connectionString,
            string databaseName
        )
        {
            _optionsFactory = optionsFactory;
            _loggerFactory = loggerFactory;
            _resolvingStrategyChain = resolvingStrategyChain;
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        public TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
        {
            return _optionsFactory.Create(transactionSessionOptionsName);
        }

        public async Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions)
        {
            var logger = _loggerFactory.CreateLogger<TEntity>();

            var mongoClient = new MongoClient(_connectionString);

            var mongoDatabase = mongoClient.GetDatabase(_databaseName);

            var clientSessionHandle = await mongoClient.StartSessionAsync(new ClientSessionOptions
            {
                CausalConsistency = true
            });

            return new MongoSession
            (
                mongoDatabase,
                clientSessionHandle,
                logger,
                _resolvingStrategyChain,
                transactionSessionOptions
            );
        }

        public ITransactionRepository<TEntity> CreateRepository
        (
            IMongoSession mongoSession
        )
        {
            var mongoDbTransactionRepository = new MongoDbTransactionRepository<TEntity>
            (
                mongoSession
            );

            return mongoDbTransactionRepository.UseTryCatch(mongoSession.Logger);
        }

        public static MongoDbTransactionRepositoryFactory<TEntity> Create(IServiceProvider serviceProvider,
            string connectionString, string databaseName)
        {
            return ActivatorUtilities.CreateInstance<MongoDbTransactionRepositoryFactory<TEntity>>
            (
                serviceProvider,
                connectionString,
                databaseName
            );
        }
    }
}
