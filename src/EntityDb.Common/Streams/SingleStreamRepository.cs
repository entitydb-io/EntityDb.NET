using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Streams;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.Streams;

internal sealed class SingleStreamRepository : DisposableResourceBaseClass, ISingleStreamRepository
{
    private readonly IMultipleStreamRepository _multipleStreamRepository;

    public SingleStreamRepository(IMultipleStreamRepository multipleStreamRepository, IStateKey streamKey)
    {
        _multipleStreamRepository = multipleStreamRepository;

        StreamKey = streamKey;
    }

    public ISourceRepository SourceRepository => _multipleStreamRepository.SourceRepository;
    public IStateKey StreamKey { get; }

    public Task<bool> Append(object delta, CancellationToken cancellationToken = default)
    {
        return _multipleStreamRepository.Append(StreamKey, delta, cancellationToken);
    }

    public Task<bool> Commit(CancellationToken cancellationToken = default)
    {
        return _multipleStreamRepository.Commit(cancellationToken);
    }

    public override ValueTask DisposeAsync()
    {
        return _multipleStreamRepository.DisposeAsync();
    }
}
