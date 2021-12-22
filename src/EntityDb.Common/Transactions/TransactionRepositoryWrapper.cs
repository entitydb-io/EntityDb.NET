using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using System;
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

        public Task<Guid[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery)
        {
            return WrapQuery(_transactionRepository.GetTransactionIds(agentSignatureQuery));
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

        public Task<Guid[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery)
        {
            return WrapQuery(_transactionRepository.GetEntityIds(agentSignatureQuery));
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

        public Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery)
        {
            return WrapQuery(_transactionRepository.GetAgentSignatures(agentSignatureQuery));
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

        public virtual ValueTask DisposeAsync()
        {
            return _transactionRepository.DisposeAsync();
        }

        protected abstract Task<T[]> WrapQuery<T>(Task<T[]> task);

        protected abstract Task<bool> WrapCommand(Task<bool> task);
    }
}
