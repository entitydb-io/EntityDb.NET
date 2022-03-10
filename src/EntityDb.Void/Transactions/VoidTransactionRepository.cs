using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System;
using System.Threading.Tasks;

namespace EntityDb.Void.Transactions;

internal sealed class VoidTransactionRepository : DisposableResourceBaseClass, ITransactionRepository
{
    private static readonly Task<Id[]> EmptyIdArrayTask = Task.FromResult(Array.Empty<Id>());
    private static readonly Task<object[]> EmptyObjectArrayTask = Task.FromResult(Array.Empty<object>());
    private static readonly Task<ILease[]> EmptyLeaseArrayTask = Task.FromResult(Array.Empty<ILease>());
    private static readonly Task<ITag[]> EmptyTagArrayTask = Task.FromResult(Array.Empty<ITag>());
    private static readonly Task<IEntityAnnotation<object>[]> EmptyEntityAnnotationArrayTask = Task.FromResult(Array.Empty<IEntityAnnotation<object>>());
    private static readonly Task<bool> TrueBoolTask = Task.FromResult(true);
    
    public Task<Id[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return EmptyIdArrayTask;
    }

    public Task<Id[]> GetTransactionIds(ICommandQuery commandQuery)
    {
        return EmptyIdArrayTask;
    }

    public Task<Id[]> GetTransactionIds(ILeaseQuery leaseQuery)
    {
        return EmptyIdArrayTask;
    }

    public Task<Id[]> GetTransactionIds(ITagQuery tagQuery)
    {
        return EmptyIdArrayTask;
    }

    public Task<Id[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return EmptyIdArrayTask;
    }

    public Task<Id[]> GetEntityIds(ICommandQuery commandQuery)
    {
        return EmptyIdArrayTask;
    }

    public Task<Id[]> GetEntityIds(ILeaseQuery leaseQuery)
    {
        return EmptyIdArrayTask;
    }

    public Task<Id[]> GetEntityIds(ITagQuery tagQuery)
    {
        return EmptyIdArrayTask;
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
