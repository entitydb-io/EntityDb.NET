using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Commands;
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
        public Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery)
        {
            return Task.FromResult(Array.Empty<Guid>());
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return Task.FromResult(Array.Empty<Guid>());
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return Task.FromResult(Array.Empty<Guid>());
        }

        public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
        {
            return Task.FromResult(Array.Empty<Guid>());
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return Task.FromResult(Array.Empty<Guid>());
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return Task.FromResult(Array.Empty<Guid>());
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return Task.FromResult(Array.Empty<Guid>());
        }

        public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
        {
            return Task.FromResult(Array.Empty<Guid>());
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return Task.FromResult(Array.Empty<object>());
        }

        public Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery)
        {
            return Task.FromResult(Array.Empty<ICommand<TEntity>>());
        }

        public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
        {
            return Task.FromResult(Array.Empty<ILease>());
        }

        public Task<ITag[]> GetTags(ITagQuery tagQuery)
        {
            return Task.FromResult(Array.Empty<ITag>());
        }

        public Task<IEntityAnnotation<ICommand<TEntity>>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
        {
            return Task.FromResult(Array.Empty<IEntityAnnotation<ICommand<TEntity>>>());
        }

        public Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            return Task.FromResult(true);
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
