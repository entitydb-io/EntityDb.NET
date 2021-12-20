using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Queries;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal class MongoDbTransactionRepository<TEntity> : ITransactionRepository<TEntity>
    {
        protected readonly IMongoSession? _mongoSession;
        protected readonly IMongoDatabase _mongoDatabase;
        protected readonly ILogger _logger;
        protected readonly IResolvingStrategyChain _resolvingStrategyChain;

        public MongoDbTransactionRepository
        (
            IMongoSession? mongoSession,
            IMongoDatabase mongoDatabase,
            ILogger logger,
            IResolvingStrategyChain resolvingStrategyChain
        )
        {
            _mongoSession = mongoSession;
            _mongoDatabase = mongoDatabase;
            _logger = logger;
            _resolvingStrategyChain = resolvingStrategyChain;
        }

        public Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery)
        {
            return SourceDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, sourceQuery)
                .GetTransactionIds();
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return CommandDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, commandQuery)
                .GetTransactionIds();
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return LeaseDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, leaseQuery)
                .GetTransactionIds();
        }

        public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
        {
            return TagDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, tagQuery)
                .GetTransactionIds();
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return SourceDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, sourceQuery)
                .GetEntitiesIds();
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return CommandDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, commandQuery)
                .GetEntityIds();
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return LeaseDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, leaseQuery)
                .GetEntityIds();
        }

        public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
        {
            return TagDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, tagQuery)
                .GetEntityIds();
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return SourceDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, sourceQuery)
                .GetData<SourceDocument, object>(_logger, _resolvingStrategyChain);
        }

        public Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery)
        {
            return CommandDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, commandQuery)
                .GetData<CommandDocument, ICommand<TEntity>>(_logger, _resolvingStrategyChain);
        }
        public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
        {
            return LeaseDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, leaseQuery)
                .GetData<LeaseDocument, ILease>(_logger, _resolvingStrategyChain);
        }

        public Task<ITag[]> GetTags(ITagQuery tagQuery)
        {
            return TagDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, tagQuery)
                .GetData<TagDocument, ITag>(_logger, _resolvingStrategyChain);
        }

        public Task<IEntityAnnotation<ICommand<TEntity>>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
        {
            return CommandDocument
                .GetDocumentQuery(_mongoSession, _mongoDatabase, commandQuery)
                .GetEntityAnnotation<CommandDocument, ICommand<TEntity>>(_logger, _resolvingStrategyChain);
        }

        public async Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            if (_mongoSession == null)
            {
                throw new CannotWriteInReadOnlyModeException();
            }

            try
            {
                _mongoSession.StartTransaction();

                await SourceDocument.InsertOne(_mongoSession, _mongoDatabase, _logger, transaction);

                foreach (var transactionStep in transaction.Steps)
                {
                    VersionZeroReservedException.ThrowIfZero(transactionStep.NextEntityVersionNumber);

                    var previousVersionNumber = await CommandDocument.GetLastEntityVersionNumber(_mongoSession, _mongoDatabase, transactionStep.EntityId);

                    OptimisticConcurrencyException.ThrowIfMismatch(previousVersionNumber, transactionStep.PreviousEntityVersionNumber);

                    await CommandDocument.InsertOne(_mongoSession, _mongoDatabase, _logger, transaction, transactionStep);

                    await LeaseDocument.DeleteMany(_mongoSession, _mongoDatabase, transactionStep.EntityId, transactionStep.Leases.Delete);

                    await LeaseDocument.InsertMany(_mongoSession, _mongoDatabase, _logger, transaction, transactionStep);

                    await TagDocument.DeleteMany(_mongoSession, _mongoDatabase, transactionStep.EntityId, transactionStep.Tags.Delete);

                    await TagDocument.InsertMany(_mongoSession, _mongoDatabase, _logger, transaction, transactionStep);
                }

                await _mongoSession.CommitTransaction();

                return true;
            }
            catch
            {
                await _mongoSession.AbortTransaction();

                throw;
            }
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            if (_mongoSession != null)
            {
                await _mongoSession.DisposeAsync();
            }
        }
    }
}
