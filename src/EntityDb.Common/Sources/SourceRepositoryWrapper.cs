using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.Sources;

internal abstract class SourceRepositoryWrapper : DisposableResourceBaseClass, ISourceRepository
{
    private readonly ISourceRepository _sourceRepository;

    protected SourceRepositoryWrapper(ISourceRepository sourceRepository)
    {
        _sourceRepository = sourceRepository;
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(IMessageGroupQuery messageGroupQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(messageGroupQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(IMessageQuery messageQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(messageQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(leaseQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(tagQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateEntityPointers(IMessageGroupQuery messageGroupQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateEntityPointers(messageGroupQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateEntityPointers(IMessageQuery messageQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateEntityPointers(messageQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateEntityPointers(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateEntityPointers(leaseQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateEntityPointers(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateEntityPointers(tagQuery, cancellationToken));
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(IMessageGroupQuery messageGroupQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateAgentSignatures(messageGroupQuery, cancellationToken));
    }

    public IAsyncEnumerable<object> EnumerateDeltas(IMessageQuery messageQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateDeltas(messageQuery, cancellationToken));
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateLeases(leaseQuery, cancellationToken));
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateTags(tagQuery, cancellationToken));
    }

    public IAsyncEnumerable<IAnnotatedSourceGroupData<object>> EnumerateAnnotatedAgentSignatures(
        IMessageGroupQuery messageGroupQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() =>
            _sourceRepository.EnumerateAnnotatedAgentSignatures(messageGroupQuery, cancellationToken));
    }

    public IAsyncEnumerable<IAnnotatedSourceData<object>> EnumerateAnnotatedDeltas(IMessageQuery messageQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateAnnotatedDeltas(messageQuery, cancellationToken));
    }

    public Task<bool> Commit(Source source,
        CancellationToken cancellationToken = default)
    {
        return WrapCommand(() => _sourceRepository.Commit(source, cancellationToken));
    }

    public override async ValueTask DisposeAsync()
    {
        await _sourceRepository.DisposeAsync();
    }

    protected abstract IAsyncEnumerable<T> WrapQuery<T>(Func<IAsyncEnumerable<T>> enumerable);

    protected abstract Task<bool> WrapCommand(Func<Task<bool>> task);
}
