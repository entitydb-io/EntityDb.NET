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

    public IAsyncEnumerable<Id> EnumerateSourceIds(ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(sourceDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(messageDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ILeaseDataDataQuery leaseDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(leaseDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ITagDataDataQuery tagDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateSourceIds(tagDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateStatePointers(sourceDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateStatePointers(messageDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ILeaseDataDataQuery leaseDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateStatePointers(leaseDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ITagDataDataQuery tagDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateStatePointers(tagDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateAgentSignatures(sourceDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<object> EnumerateDeltas(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateDeltas(messageDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseDataDataQuery leaseDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateLeases(leaseDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagDataDataQuery tagDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateTags(tagDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<IAnnotatedSourceData<object>> EnumerateAnnotatedAgentSignatures(
        ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() =>
            _sourceRepository.EnumerateAnnotatedAgentSignatures(sourceDataDataQuery, cancellationToken));
    }

    public IAsyncEnumerable<IAnnotatedMessageData<object>> EnumerateAnnotatedDeltas(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _sourceRepository.EnumerateAnnotatedDeltas(messageDataDataQuery, cancellationToken));
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
