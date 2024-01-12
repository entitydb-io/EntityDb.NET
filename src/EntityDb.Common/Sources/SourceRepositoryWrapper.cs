using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.States.Attributes;
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

    public IAsyncEnumerable<Id> EnumerateSourceIds(ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(sourceDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(messageDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ILeaseDataQuery leaseDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(leaseDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ITagDataQuery tagDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(tagDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateStatePointers(sourceDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateStatePointers(messageDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ILeaseDataQuery leaseDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateStatePointers(leaseDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ITagDataQuery tagDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateStatePointers(tagDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateAgentSignatures(sourceDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<object> EnumerateDeltas(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateDeltas(messageDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseDataQuery leaseDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateLeases(leaseDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagDataQuery tagDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateTags(tagDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<IAnnotatedSourceData<object>> EnumerateAnnotatedAgentSignatures(
        ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() =>
            _sourceRepository.EnumerateAnnotatedAgentSignatures(sourceDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<IAnnotatedMessageData<object>> EnumerateAnnotatedDeltas(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateAnnotatedDeltas(messageDataQuery, cancellationToken));
    }

    public virtual Task<bool> Commit(Source source,
        CancellationToken cancellationToken = default)
    {
        return WrapCommand(() => _sourceRepository.Commit(source, cancellationToken));
    }

    public override async ValueTask DisposeAsync()
    {
        await _sourceRepository.DisposeAsync();
    }

    protected virtual IAsyncEnumerable<T> WrapQuery<T>(Func<IAsyncEnumerable<T>> enumerable)
    {
        return enumerable.Invoke();
    }

    protected virtual Task<bool> WrapCommand(Func<Task<bool>> task)
    {
        return task.Invoke();
    }
}
