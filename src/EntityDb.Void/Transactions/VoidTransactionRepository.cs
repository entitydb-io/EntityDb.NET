using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using System;
using System.Threading.Tasks;

namespace EntityDb.Void.Transactions;

internal sealed class VoidTransactionRepository<TEntity> : DisposableResourceBaseClass, ITransactionRepository<TEntity>
{
    public Task<Guid[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery)
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

    public Task<Guid[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery)
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

    public Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery)
    {
        return Task.FromResult(Array.Empty<object>());
    }

    public Task<object[]> GetCommands(ICommandQuery commandQuery)
    {
        return Task.FromResult(Array.Empty<object>());
    }

    public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
    {
        return Task.FromResult(Array.Empty<ILease>());
    }

    public Task<ITag[]> GetTags(ITagQuery tagQuery)
    {
        return Task.FromResult(Array.Empty<ITag>());
    }

    public Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
    {
        return Task.FromResult(Array.Empty<IEntityAnnotation<object>>());
    }

    public Task<bool> PutTransaction(ITransaction<TEntity> transaction)
    {
        return Task.FromResult(true);
    }
}
