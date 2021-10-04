using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Void.Transactions
{
    internal sealed class VoidTransactionRepository<TEntity> : ITransactionRepository<TEntity>
    {
        private static readonly Task<Guid[]> _emptyGuidArrayTask = Task.FromResult(Array.Empty<Guid>());
        private static readonly Task<object[]> _emptyObjectArrayTask = Task.FromResult(Array.Empty<object>());

        private static readonly Task<ICommand<TEntity>[]> _emptyCommandArrayTask =
            Task.FromResult(Array.Empty<ICommand<TEntity>>());

        private static readonly Task<IFact<TEntity>[]> _emptyFactArrayTask =
            Task.FromResult(Array.Empty<IFact<TEntity>>());

        private static readonly Task<ILease[]> _emptyLeaseArrayTask = Task.FromResult(Array.Empty<ILease>());
        private static readonly Task<ITag[]> _emptyTagArrayTask = Task.FromResult(Array.Empty<ITag>());
        private static readonly Task<bool> _trueTask = Task.FromResult(true);

        public Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<Guid[]> GetTransactionIds(IFactQuery factQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(IFactQuery factQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
        {
            return _emptyGuidArrayTask;
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return _emptyObjectArrayTask;
        }

        public Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery)
        {
            return _emptyCommandArrayTask;
        }

        public Task<IFact<TEntity>[]> GetFacts(IFactQuery factQuery)
        {
            return _emptyFactArrayTask;
        }

        public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
        {
            return _emptyLeaseArrayTask;
        }

        public Task<ITag[]> GetTags(ITagQuery tagQuery)
        {
            return _emptyTagArrayTask;
        }

        public Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            return _trueTask;
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
