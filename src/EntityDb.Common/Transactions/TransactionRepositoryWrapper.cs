using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions
{
    internal abstract class TransactionRepositoryWrapper<TEntity> : ITransactionRepository<TEntity>
    {
        private readonly ITransactionRepository<TEntity> _transactionRepository;

        protected TransactionRepositoryWrapper(ITransactionRepository<TEntity> transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery)
        {
            return WrapQuery(_transactionRepository.GetTransactionIds(sourceQuery));
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return WrapQuery(_transactionRepository.GetTransactionIds(commandQuery));
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return WrapQuery(_transactionRepository.GetTransactionIds(leaseQuery));
        }

        public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
        {
            return WrapQuery(_transactionRepository.GetTransactionIds(tagQuery));
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return WrapQuery(_transactionRepository.GetEntityIds(sourceQuery));
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return WrapQuery(_transactionRepository.GetEntityIds(commandQuery));
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return WrapQuery(_transactionRepository.GetEntityIds(leaseQuery));
        }

        public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
        {
            return WrapQuery(_transactionRepository.GetEntityIds(tagQuery));
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return WrapQuery(_transactionRepository.GetSources(sourceQuery));
        }

        public Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery)
        {
            return WrapQuery(_transactionRepository.GetCommands(commandQuery));
        }

        public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
        {
            return WrapQuery(_transactionRepository.GetLeases(leaseQuery));
        }

        public Task<ITag[]> GetTags(ITagQuery tagQuery)
        {
            return WrapQuery(_transactionRepository.GetTags(tagQuery));
        }

        public Task<IEntityAnnotation<ICommand<TEntity>>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
        {
            return WrapQuery(_transactionRepository.GetAnnotatedCommands(commandQuery));
        }

        public Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            return WrapCommand(_transactionRepository.PutTransaction(transaction));
        }

        [ExcludeFromCodeCoverage(Justification = "Proxy for DisposeAsync")]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public virtual ValueTask DisposeAsync()
        {
            return _transactionRepository.DisposeAsync();
        }

        protected virtual Task<T[]> WrapQuery<T>(Task<T[]> task)
        {
            return task;
        }

        protected virtual Task<bool> WrapCommand(Task<bool> task)
        {
            return task;
        }
    }
}
