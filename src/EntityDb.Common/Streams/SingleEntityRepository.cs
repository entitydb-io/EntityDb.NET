using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Streams;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.Streams;

internal sealed class SingleStreamRepository : DisposableResourceBaseClass, ISingleStreamRepository
{
    private readonly IMultipleStreamRepository _multipleStreamRepository;

    public SingleStreamRepository(IMultipleStreamRepository multipleStreamRepository, Key streamKey)
    {
        _multipleStreamRepository = multipleStreamRepository;

        StreamKey = streamKey;
    }

    public ISourceRepository SourceRepository => _multipleStreamRepository.SourceRepository;
    public Key StreamKey { get; }

    public Task<bool> Stage(Key messageKey, object delta, CancellationToken cancellationToken = default)
    {
        return _multipleStreamRepository.Append(StreamKey, messageKey, delta, cancellationToken);
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
