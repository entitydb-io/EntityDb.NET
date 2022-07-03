using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Void.Transactions;

internal sealed class VoidTransactionRepository : DisposableResourceBaseClass, ITransactionRepository
{
    private static readonly Task<Id[]> EmptyIdArrayTask = Task.FromResult(Array.Empty<Id>());
    private static readonly Task<object[]> EmptyObjectArrayTask = Task.FromResult(Array.Empty<object>());
    private static readonly Task<ILease[]> EmptyLeaseArrayTask = Task.FromResult(Array.Empty<ILease>());
    private static readonly Task<ITag[]> EmptyTagArrayTask = Task.FromResult(Array.Empty<ITag>());
    private static readonly Task<IEntitiesAnnotation<object>[]> EmptyEntitiesAnnotationArrayTask = Task.FromResult(Array.Empty<IEntitiesAnnotation<object>>());
    private static readonly Task<IEntityAnnotation<object>[]> EmptyEntityAnnotationArrayTask = Task.FromResult(Array.Empty<IEntityAnnotation<object>>());
    private static readonly Task<bool> TrueBoolTask = Task.FromResult(true);

    public Task<Id[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return EmptyIdArrayTask.WaitAsync(cancellationToken);
    }

    public Task<Id[]> GetTransactionIds(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return EmptyIdArrayTask.WaitAsync(cancellationToken);
    }

    public Task<Id[]> GetTransactionIds(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default)
    {
        return EmptyIdArrayTask.WaitAsync(cancellationToken);
    }

    public Task<Id[]> GetTransactionIds(ITagQuery tagQuery, CancellationToken cancellationToken = default)
    {
        return EmptyIdArrayTask.WaitAsync(cancellationToken);
    }

    public Task<Id[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return EmptyIdArrayTask.WaitAsync(cancellationToken);
    }

    public Task<Id[]> GetEntityIds(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return EmptyIdArrayTask.WaitAsync(cancellationToken);
    }

    public Task<Id[]> GetEntityIds(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default)
    {
        return EmptyIdArrayTask.WaitAsync(cancellationToken);
    }

    public Task<Id[]> GetEntityIds(ITagQuery tagQuery, CancellationToken cancellationToken = default)
    {
        return EmptyIdArrayTask.WaitAsync(cancellationToken);
    }

    public Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return EmptyObjectArrayTask.WaitAsync(cancellationToken);
    }

    public Task<object[]> GetCommands(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return EmptyObjectArrayTask.WaitAsync(cancellationToken);
    }

    public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default)
    {
        return EmptyLeaseArrayTask.WaitAsync(cancellationToken);
    }

    public Task<ITag[]> GetTags(ITagQuery tagQuery, CancellationToken cancellationToken = default)
    {
        return EmptyTagArrayTask.WaitAsync(cancellationToken);
    }

    public Task<IEntitiesAnnotation<object>[]> GetAnnotatedAgentSignatures(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return EmptyEntitiesAnnotationArrayTask.WaitAsync(cancellationToken);
    }

    public Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return EmptyEntityAnnotationArrayTask.WaitAsync(cancellationToken);
    }

    public Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default)
    {
        return TrueBoolTask.WaitAsync(cancellationToken);
    }
}
