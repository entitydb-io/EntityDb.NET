using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Subjects;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Void.Transactions;

internal sealed class VoidSourceRepository : DisposableResourceBaseClass, ISourceRepository
{
    public IAsyncEnumerable<Id> EnumerateSourceIds(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ISourceSubjectQuery sourceSubjectQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Id>();
    }

    public IAsyncEnumerable<Pointer> EnumerateSubjectPointers(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Pointer>();
    }

    public IAsyncEnumerable<Pointer> EnumerateSubjectPointers(ISourceSubjectQuery sourceSubjectQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Pointer>();
    }

    public IAsyncEnumerable<Pointer> EnumerateSubjectPointers(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Pointer>();
    }

    public IAsyncEnumerable<Pointer> EnumerateSubjectPointers(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<Pointer>();
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<object>();
    }

    public IAsyncEnumerable<object> EnumerateSubjects(ISourceSubjectQuery sourceSubjectQuery,
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

    public IAsyncEnumerable<IAnnotatedSourceSubjects<object>> EnumerateAnnotatedAgentSignatures(
        IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<IAnnotatedSourceSubjects<object>>();
    }

    public IAsyncEnumerable<IAnnotatedSourceSubject<object>> EnumerateAnnotatedSubjects(ISourceSubjectQuery sourceSubjectQuery,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<IAnnotatedSourceSubject<object>>();
    }

    public Task<bool> Put(ISource source,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
