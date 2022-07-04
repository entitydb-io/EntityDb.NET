using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Void.Transactions;

internal sealed class VoidTransactionRepository : DisposableResourceBaseClass, ITransactionRepository
{
    public IAsyncEnumerable<Id> EnumerateTransactionIds(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<object>();
    }

    public IAsyncEnumerable<object> EnumerateCommands(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<object>();
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<ILease>();
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<ITag>();
    }

    public IAsyncEnumerable<IEntitiesAnnotation<object>> EnumerateAnnotatedAgentSignatures(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<IEntitiesAnnotation<object>>();
    }

    public IAsyncEnumerable<IEntityAnnotation<object>> EnumerateAnnotatedCommands(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<IEntityAnnotation<object>>();
    }

    public Task<bool> PutTransaction(ITransaction transaction,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
