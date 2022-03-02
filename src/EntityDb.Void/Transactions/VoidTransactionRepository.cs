using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using System;
using System.Threading.Tasks;

namespace EntityDb.Void.Transactions;

internal sealed class VoidTransactionRepository : DisposableResourceBaseClass, ITransactionRepository
{
    private static readonly Task<Guid[]> EmptyGuidArrayTask = Task.FromResult(Array.Empty<Guid>());
    private static readonly Task<object[]> EmptyObjectArrayTask = Task.FromResult(Array.Empty<object>());
    private static readonly Task<ILease[]> EmptyLeaseArrayTask = Task.FromResult(Array.Empty<ILease>());
    private static readonly Task<ITag[]> EmptyTagArrayTask = Task.FromResult(Array.Empty<ITag>());
    private static readonly Task<IEntityAnnotation<object>[]> EmptyEntityAnnotationArrayTask = Task.FromResult(Array.Empty<IEntityAnnotation<object>>());
    private static readonly Task<bool> TrueBoolTask = Task.FromResult(true);
    
    public Task<Guid[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return EmptyGuidArrayTask;
    }

    public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
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

    public Task<Guid[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return EmptyGuidArrayTask;
    }

    public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
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

    public Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery)
    {
        return EmptyObjectArrayTask;
    }

    public Task<object[]> GetCommands(ICommandQuery commandQuery)
    {
        return EmptyObjectArrayTask;
    }

    public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
    {
        return EmptyLeaseArrayTask;
    }

    public Task<ITag[]> GetTags(ITagQuery tagQuery)
    {
        return EmptyTagArrayTask;
    }

    public Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
    {
        return EmptyEntityAnnotationArrayTask;
    }

    public Task<bool> PutTransaction(ITransaction transaction)
    {
        return TrueBoolTask;
    }
}
