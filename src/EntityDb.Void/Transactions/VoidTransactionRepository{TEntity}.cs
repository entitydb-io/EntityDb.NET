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
    internal sealed class VoidTransactionRepository<TEntity> : VoidTransactionRepository,
        ITransactionRepository<TEntity>
    {
        private static readonly Task<ICommand<TEntity>[]> _emptyCommandArrayTask =
            Task.FromResult(Array.Empty<ICommand<TEntity>>());

        private static readonly Task<IFact<TEntity>[]> _emptyFactArrayTask =
            Task.FromResult(Array.Empty<IFact<TEntity>>());

        public Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<Guid[]> GetTransactionIds(IFactQuery factQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(IFactQuery factQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
        {
            return EmptyGuidArrayTask;
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return EmptyObjectArrayTask;
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
            return EmptyLeaseArrayTask;
        }

        public Task<ITag[]> GetTags(ITagQuery tagQuery)
        {
            return EmptyTagArrayTask;
        }

        public Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            return TrueTask;
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
