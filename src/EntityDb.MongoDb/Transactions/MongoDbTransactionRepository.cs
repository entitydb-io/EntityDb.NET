using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Sessions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class MongoDbTransactionRepository<TEntity> : ITransactionRepository<TEntity>
    {
        protected readonly IMongoSession _mongoSession;

        public MongoDbTransactionRepository
        (
            IMongoSession mongoSession
        )
        {
            _mongoSession = mongoSession;
        }

        public Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery)
        {
            return SourceDocument
                .GetQuery(_mongoSession, sourceQuery)
                .GetTransactionIds();
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return CommandDocument
                .GetQuery(_mongoSession, commandQuery)
                .GetTransactionIds();
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return LeaseDocument
                .GetQuery(_mongoSession, leaseQuery)
                .GetTransactionIds();
        }

        public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
        {
            return TagDocument
                .GetQuery(_mongoSession, tagQuery)
                .GetTransactionIds();
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return SourceDocument
                .GetQuery(_mongoSession, sourceQuery)
                .GetEntitiesIds();
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return CommandDocument
                .GetQuery(_mongoSession, commandQuery)
                .GetEntityIds();
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return LeaseDocument
                .GetQuery(_mongoSession, leaseQuery)
                .GetEntityIds();
        }

        public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
        {
            return TagDocument
                .GetQuery(_mongoSession, tagQuery)
                .GetEntityIds();
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return SourceDocument
                .GetQuery(_mongoSession, sourceQuery)
                .GetData<SourceDocument, object>();
        }

        public Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery)
        {
            return CommandDocument
                .GetQuery(_mongoSession, commandQuery)
                .GetData<CommandDocument, ICommand<TEntity>>();
        }

        public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
        {
            return LeaseDocument
                .GetQuery(_mongoSession, leaseQuery)
                .GetData<LeaseDocument, ILease>();
        }

        public Task<ITag[]> GetTags(ITagQuery tagQuery)
        {
            return TagDocument
                .GetQuery(_mongoSession, tagQuery)
                .GetData<TagDocument, ITag>();
        }

        public Task<IEntityAnnotation<ICommand<TEntity>>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
        {
            return CommandDocument
                .GetQuery(_mongoSession, commandQuery)
                .GetEntityAnnotation<CommandDocument, ICommand<TEntity>>();
        }

        public async Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            try
            {
                _mongoSession.StartTransaction();

                await SourceDocument
                    .GetInsertCommand<TEntity>(_mongoSession)
                    .Execute(transaction, -1);

                for (var transactionStepIndex = 0; transactionStepIndex < transaction.Steps.Length; transactionStepIndex++)
                {
                    var transactionStep = transaction.Steps[transactionStepIndex];

                    VersionZeroReservedException.ThrowIfZero(transaction.Steps[transactionStepIndex].NextEntityVersionNumber);

                    var previousVersionNumber = await CommandDocument.GetLastEntityVersionNumber(_mongoSession, transactionStep.EntityId);

                    OptimisticConcurrencyException.ThrowIfMismatch(previousVersionNumber, transactionStep.PreviousEntityVersionNumber);

                    await CommandDocument.GetInsertCommand<TEntity>(_mongoSession).Execute(transaction, transactionStepIndex);

                    await LeaseDocument.GetDeleteCommand<TEntity>(_mongoSession).Execute(transaction, transactionStepIndex);

                    await LeaseDocument.GetInsertCommand<TEntity>(_mongoSession).Execute(transaction, transactionStepIndex);

                    await TagDocument.GetDeleteCommand<TEntity>(_mongoSession).Execute(transaction, transactionStepIndex);

                    await TagDocument.GetInsertCommand<TEntity>(_mongoSession).Execute(transaction, transactionStepIndex);
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

        [ExcludeFromCodeCoverage(Justification = "Proxy for DisposeAsync")]
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
