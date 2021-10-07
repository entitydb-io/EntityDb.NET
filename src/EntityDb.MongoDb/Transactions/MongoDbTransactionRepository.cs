using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Sessions;
using System;
using System.Diagnostics.CodeAnalysis;
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
            return SourceDocument.GetTransactionIds(_mongoDbSession, sourceQuery);
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return CommandDocument.GetTransactionIds(_mongoDbSession, commandQuery);
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return LeaseDocument.GetTransactionIds(_mongoDbSession, leaseQuery);
        }

        public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
        {
            return TagDocument.GetTransactionIds(_mongoDbSession, tagQuery);
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return SourceDocument.GetEntityIds(_mongoDbSession, sourceQuery);
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return CommandDocument.GetEntityIds(_mongoDbSession, commandQuery);
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return LeaseDocument.GetEntityIds(_mongoDbSession, leaseQuery);
        }

        public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
        {
            return TagDocument.GetEntityIds(_mongoDbSession, tagQuery);
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return SourceDocument.GetData(_mongoDbSession, sourceQuery);
        }

        public Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery)
        {
            return CommandDocument.GetData<TEntity>(_mongoDbSession, commandQuery);
        }

        public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
        {
            return LeaseDocument.GetData(_mongoDbSession, leaseQuery);
        }

        public Task<ITag[]> GetTags(ITagQuery tagQuery)
        {
            return TagDocument.GetData(_mongoDbSession, tagQuery);
        }

        public Task<IAnnotatedCommand<TEntity>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
        {
            return CommandDocument.GetAnnotated<TEntity>(_mongoDbSession, commandQuery);
        }

        public Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            return _mongoDbSession.ExecuteCommand
            (
                async (logger, clientSessionHandle, mongoDatabase) =>
                {
                    await SourceDocument.InsertOne(clientSessionHandle, mongoDatabase,
                        SourceDocument.BuildOne(logger, transaction));

                    foreach (var transactionCommand in transaction.Commands)
                    {
                        VersionZeroReservedException.ThrowIfZero(transactionCommand.EntityVersionNumber);
                        
                        var previousVersionNumber =
                            await CommandDocument.GetLastEntityVersionNumber(clientSessionHandle, mongoDatabase,
                                transactionCommand.EntityId);

                        OptimisticConcurrencyException.ThrowIfDiscontinuous(previousVersionNumber, transactionCommand.EntityVersionNumber);

                        await CommandDocument.InsertOne(clientSessionHandle, mongoDatabase,
                            CommandDocument.BuildOne(logger, transaction, transactionCommand));
                        await LeaseDocument.DeleteMany(clientSessionHandle, mongoDatabase, transactionCommand.EntityId,
                            transactionCommand.Leases.Delete);
                        await LeaseDocument.InsertMany(clientSessionHandle, mongoDatabase,
                            LeaseDocument.BuildMany(logger, transaction, transactionCommand));
                        await TagDocument.DeleteMany(clientSessionHandle, mongoDatabase, transactionCommand.EntityId,
                            transactionCommand.Tags.Delete);
                        await TagDocument.InsertMany(clientSessionHandle, mongoDatabase,
                            TagDocument.BuildMany(logger, transaction, transactionCommand));
                    }
                }
            );
        }

        [ExcludeFromCodeCoverage]
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
