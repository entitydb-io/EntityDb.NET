using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal sealed class MongoDbTransactionRepository<TEntity> : ITransactionRepository<TEntity>
    {
        private readonly IMongoDbSession _mongoDbSession;

        public MongoDbTransactionRepository(IMongoDbSession mongoDbSession)
        {
            _mongoDbSession = mongoDbSession;
        }

        public Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (serviceProvider, clientSessionHandle, mongoDatabase) => await SourceDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, sourceQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (serviceProvider, clientSessionHandle, mongoDatabase) => await CommandDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, commandQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetTransactionIds(IFactQuery factQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (serviceProvider, clientSessionHandle, mongoDatabase) => await FactDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, factQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (serviceProvider, clientSessionHandle, mongoDatabase) => await LeaseDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, leaseQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (serviceProvider, clientSessionHandle, mongoDatabase) => await SourceDocument.GetEntityIds(clientSessionHandle, mongoDatabase, sourceQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (serviceProvider, clientSessionHandle, mongoDatabase) => await CommandDocument.GetEntityIds(clientSessionHandle, mongoDatabase, commandQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetEntityIds(IFactQuery factQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (serviceProvider, clientSessionHandle, mongoDatabase) => await FactDocument.GetEntityIds(clientSessionHandle, mongoDatabase, factQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (serviceProvider, clientSessionHandle, mongoDatabase) => await LeaseDocument.GetEntityIds(clientSessionHandle, mongoDatabase, leaseQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                (serviceProvider, clientSessionHandle, mongoDatabase) => SourceDocument.GetSources(serviceProvider, clientSessionHandle, mongoDatabase, sourceQuery),
                Array.Empty<object>()
            );
        }

        public Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                (serviceProvider, clientSessionHandle, mongoDatabase) => CommandDocument.GetCommands<TEntity>(serviceProvider, clientSessionHandle, mongoDatabase, commandQuery),
                Array.Empty<ICommand<TEntity>>()
            );
        }

        public Task<IFact<TEntity>[]> GetFacts(IFactQuery factQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                (serviceProvider, clientSessionHandle, mongoDatabase) => FactDocument.GetFacts<TEntity>(serviceProvider, clientSessionHandle, mongoDatabase, factQuery),
                Array.Empty<IFact<TEntity>>()
            );
        }

        public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                (serviceProvider, clientSessionHandle, mongoDatabase) => LeaseDocument.GetLeases(serviceProvider, clientSessionHandle, mongoDatabase, leaseQuery),
                Array.Empty<ILease>()
            );
        }

        public Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            return _mongoDbSession.ExecuteCommand
            (
                async (serviceProvider, clientSessionHandle, mongoDatabase) =>
                {
                    var entityIds = transaction.Commands
                        .Select(command => command.EntityId)
                        .Distinct()
                        .ToArray();

                    await SourceDocument.InsertOne
                    (
                        serviceProvider,
                        clientSessionHandle,
                        mongoDatabase,
                        transaction.TimeStamp,
                        transaction.Id,
                        entityIds,
                        transaction.Source
                    );

                    foreach (var transactionCommand in transaction.Commands)
                    {
                        var actualPreviousVersion = await CommandDocument.GetPreviousVersionNumber
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            transactionCommand.EntityId
                        );

                        if (transactionCommand.ExpectedPreviousVersionNumber != actualPreviousVersion)
                        {
                            throw new OptimisticConcurrencyException();
                        }

                        var nextVersionNumber = transactionCommand.ExpectedPreviousVersionNumber + 1;

                        await CommandDocument.InsertOne
                        (
                            serviceProvider,
                            clientSessionHandle,
                            mongoDatabase,
                            transaction.TimeStamp,
                            transaction.Id,
                            transactionCommand.EntityId,
                            nextVersionNumber,
                            transactionCommand.Command
                        );

                        await FactDocument.InsertMany
                        (
                            serviceProvider,
                            clientSessionHandle,
                            mongoDatabase,
                            transaction.TimeStamp,
                            transaction.Id,
                            transactionCommand.EntityId,
                            nextVersionNumber,
                            transactionCommand.Facts
                        );

                        await LeaseDocument.DeleteMany
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            transactionCommand.EntityId,
                            transactionCommand.DeleteLeases
                        );

                        await LeaseDocument.InsertMany
                        (
                            serviceProvider,
                            clientSessionHandle,
                            mongoDatabase,
                            transaction.TimeStamp,
                            transaction.Id,
                            transactionCommand.EntityId,
                            nextVersionNumber,
                            transactionCommand.InsertLeases
                        );
                    }
                }
            );
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await _mongoDbSession.DisposeAsync();
        }
    }
}
